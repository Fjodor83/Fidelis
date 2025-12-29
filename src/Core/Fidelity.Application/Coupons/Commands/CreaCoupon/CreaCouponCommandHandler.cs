using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.Common.Models;
using Fidelity.Domain.Entities;
using Fidelity.Domain.Events;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fidelity.Application.Coupons.Commands.CreaCoupon;

public class CreaCouponCommandHandler : IRequestHandler<CreaCouponCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CreaCouponCommandHandler> _logger;

    public CreaCouponCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        ICurrentUserService currentUserService,
        ILogger<CreaCouponCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(CreaCouponCommand request, CancellationToken cancellationToken)
    {
        // Check if code already exists
        var codeExists = await _context.Coupons
            .AnyAsync(c => c.Codice == request.Codice.ToUpper() && !c.IsDeleted, cancellationToken);

        if (codeExists)
        {
            return Result<int>.Failure("Esiste già un coupon con questo codice.");
        }

        var tipoSconto = request.TipoSconto == "Percentuale" ? TipoSconto.Percentuale : TipoSconto.Fisso;

        var coupon = new Coupon
        {
            Codice = request.Codice.ToUpper(),
            Titolo = request.Titolo,
            Descrizione = request.Descrizione,
            ValoreSconto = request.ValoreSconto,
            TipoSconto = tipoSconto,
            DataInizio = request.DataInizio,
            DataScadenza = request.DataScadenza,
            Attivo = request.Attivo,
            AssegnazioneAutomatica = request.AssegnazioneAutomatica,
            LimiteUtilizzoGlobale = request.LimiteUtilizzoGlobale,
            LimiteUtilizzoPerCliente = request.LimiteUtilizzoPerCliente,
            ImportoMinimoOrdine = request.ImportoMinimoOrdine,
            PuntiRichiesti = request.PuntiRichiesti,
            IsCouponBenvenuto = request.IsCouponBenvenuto,
            CreatedBy = _currentUserService.Username
        };

        coupon.Validate();

        _context.Coupons.Add(coupon);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Coupon {CouponId} ({Codice}) creato da {User}",
            coupon.Id, coupon.Codice, _currentUserService.Username);

        // Auto-assign to all active clients (background job)
        if (coupon.Attivo && coupon.AssegnazioneAutomatica && !coupon.IsCouponBenvenuto)
        {
            _ = Task.Run(async () =>
            {
                await AssegnaCouponAClientiAttiviAsync(coupon, cancellationToken);
            });
        }

        return Result<int>.Success(coupon.Id, "Coupon creato con successo.");
    }

    private async Task AssegnaCouponAClientiAttiviAsync(Coupon coupon, CancellationToken cancellationToken)
    {
        try
        {
            var clientiAttivi = await _context.Clienti
                .Where(c => c.Attivo && !c.IsDeleted && !string.IsNullOrEmpty(c.Email))
                .ToListAsync(CancellationToken.None);

            _logger.LogInformation("Assegnazione coupon {Codice} a {Count} clienti attivi",
                coupon.Codice, clientiAttivi.Count);

            var batchSize = 50;
            var batches = clientiAttivi
                .Select((c, i) => new { Cliente = c, Index = i })
                .GroupBy(x => x.Index / batchSize)
                .Select(g => g.Select(x => x.Cliente).ToList());

            foreach (var batch in batches)
            {
                var assegnazioni = batch.Select(cliente =>
                    CouponAssegnato.Crea(coupon.Id, cliente.Id, MotivoAssegnazione.Automatico, "Sistema"))
                    .ToList();

                foreach (var assegnazione in assegnazioni)
                {
                    _context.CouponAssegnati.Add(assegnazione);
                }

                await _context.SaveChangesAsync(CancellationToken.None);

                // Send emails in parallel
                var emailTasks = batch.Select(cliente =>
                    _emailService.InviaEmailNuovoCouponAsync(
                        cliente.Email,
                        cliente.Nome,
                        coupon.Titolo,
                        coupon.Codice,
                        coupon.DataScadenza));

                await Task.WhenAll(emailTasks);

                _logger.LogDebug("Batch di {Count} assegnazioni completato per coupon {Codice}",
                    batch.Count, coupon.Codice);
            }

            _logger.LogInformation("Assegnazione coupon {Codice} completata: {Count} clienti",
                coupon.Codice, clientiAttivi.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Errore durante assegnazione automatica coupon {Codice}", coupon.Codice);
        }
    }
}
