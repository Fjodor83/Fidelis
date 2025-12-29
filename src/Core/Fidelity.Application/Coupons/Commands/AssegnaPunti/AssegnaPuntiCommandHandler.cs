using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.Common.Models;
using Fidelity.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fidelity.Application.Transazioni.Commands.AssegnaPunti;

public class AssegnaPuntiCommandHandler : IRequestHandler<AssegnaPuntiCommand, Result<AssegnaPuntiResponse>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<AssegnaPuntiCommandHandler> _logger;

    public AssegnaPuntiCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        ILogger<AssegnaPuntiCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
    }

    public async Task<Result<AssegnaPuntiResponse>> Handle(
        AssegnaPuntiCommand request,
        CancellationToken cancellationToken)
    {
        var cliente = await _context.Clienti
            .FirstOrDefaultAsync(c => c.Id == request.ClienteId && c.Attivo && !c.IsDeleted, cancellationToken);

        if (cliente == null)
            return Result<AssegnaPuntiResponse>.Failure("Cliente non trovato o non attivo.");

        var puntoVendita = await _context.PuntiVendita
            .FirstOrDefaultAsync(p => p.Id == request.PuntoVenditaId && p.Attivo && !p.IsDeleted, cancellationToken);

        if (puntoVendita == null)
            return Result<AssegnaPuntiResponse>.Failure("Punto vendita non trovato.");

        // Calculate points based on store configuration
        var puntiAssegnati = puntoVendita.CalcolaPunti(request.ImportoSpesa);

        if (puntiAssegnati <= 0)
            return Result<AssegnaPuntiResponse>.Failure("Importo insufficiente per assegnare punti.");

        // Track old level for comparison
        var vecchioLivello = cliente.Livello;

        // Create transaction
        var transazione = Transazione.CreaAccumulo(
            request.ClienteId,
            request.PuntoVenditaId,
            request.ImportoSpesa,
            puntiAssegnati,
            request.ResponsabileId,
            request.Note);

        _context.Transazioni.Add(transazione);
        await _context.SaveChangesAsync(cancellationToken);

        // Add points to cliente (this may trigger level up event)
        cliente.AggiungiPunti(puntiAssegnati, transazione.Id);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Assegnati {Punti} punti a cliente {ClienteId} per spesa €{Importo} presso PV {PuntoVenditaId}",
            puntiAssegnati, request.ClienteId, request.ImportoSpesa, request.PuntoVenditaId);

        // Determine if level changed
        string? nuovoLivello = null;
        if (cliente.Livello != vecchioLivello)
        {
            nuovoLivello = cliente.Livello.ToString();

            // Send level-up email
            _ = Task.Run(async () =>
            {
                await _emailService.InviaEmailLivelloRaggiuntoAsync(
                    cliente.Email, cliente.Nome, nuovoLivello);
            });
        }

        // Send points notification email
        _ = Task.Run(async () =>
        {
            await _emailService.InviaEmailPuntiAssegnatiAsync(
                cliente.Email,
                cliente.Nome,
                puntiAssegnati,
                cliente.PuntiTotali,
                request.ImportoSpesa);
        });

        return Result<AssegnaPuntiResponse>.Success(new AssegnaPuntiResponse
        {
            TransazioneId = transazione.Id,
            PuntiAssegnati = puntiAssegnati,
            PuntiTotaliCliente = cliente.PuntiTotali,
            NuovoLivello = nuovoLivello
        }, $"Assegnati {puntiAssegnati} punti.");
    }
}
