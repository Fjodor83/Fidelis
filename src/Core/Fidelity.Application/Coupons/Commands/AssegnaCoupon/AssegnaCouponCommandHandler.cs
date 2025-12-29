using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.Common.Models;
using Fidelity.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fidelity.Application.Coupons.Commands.AssegnaCoupon;

public class AssegnaCouponCommandHandler : IRequestHandler<AssegnaCouponCommand, Result<int>>
{
    private readonly IApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<AssegnaCouponCommandHandler> _logger;

    public AssegnaCouponCommandHandler(
        IApplicationDbContext context,
        IEmailService emailService,
        ICurrentUserService currentUserService,
        ILogger<AssegnaCouponCommandHandler> logger)
    {
        _context = context;
        _emailService = emailService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<int>> Handle(AssegnaCouponCommand request, CancellationToken cancellationToken)
    {
        var coupon = await _context.Coupons
            .FirstOrDefaultAsync(c => c.Id == request.CouponId && !c.IsDeleted, cancellationToken);

        if (coupon == null)
            return Result<int>.Failure("Coupon non trovato.");

        if (!coupon.IsValido())
            return Result<int>.Failure("Coupon non valido o scaduto.");

        var cliente = await _context.Clienti
            .FirstOrDefaultAsync(c => c.Id == request.ClienteId && c.Attivo && !c.IsDeleted, cancellationToken);

        if (cliente == null)
            return Result<int>.Failure("Cliente non trovato.");

        // Check if already assigned
        var giaAssegnato = await _context.CouponAssegnati
            .AnyAsync(ca => ca.CouponId == request.CouponId &&
                           ca.ClienteId == request.ClienteId &&
                           !ca.Utilizzato, cancellationToken);

        if (giaAssegnato)
            return Result<int>.Failure("Coupon già assegnato a questo cliente.");

        // Check per-customer limit
        if (coupon.LimiteUtilizzoPerCliente.HasValue)
        {
            var utilizziCliente = await _context.CouponAssegnati
                .CountAsync(ca => ca.CouponId == request.CouponId &&
                                 ca.ClienteId == request.ClienteId, cancellationToken);

            if (utilizziCliente >= coupon.LimiteUtilizzoPerCliente.Value)
                return Result<int>.Failure($"Limite utilizzo per cliente raggiunto ({coupon.LimiteUtilizzoPerCliente.Value}).");
        }

        // Check if customer meets requirements
        if (!coupon.PuoEssereAssegnatoA(cliente))
            return Result<int>.Failure("Il cliente non soddisfa i requisiti per questo coupon.");

        var motivo = (MotivoAssegnazione)(int)request.Motivo;

        var assegnazione = CouponAssegnato.Crea(
            request.CouponId,
            request.ClienteId,
            motivo,
            _currentUserService.Username);

        _context.CouponAssegnati.Add(assegnazione);
        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Coupon {CouponId} assegnato a cliente {ClienteId} da {User}",
            request.CouponId, request.ClienteId, _currentUserService.Username);

        // Send notification email
        _ = Task.Run(async () =>
        {
            await _emailService.InviaEmailNuovoCouponAsync(
                cliente.Email,
                cliente.Nome,
                coupon.Titolo,
                coupon.Codice,
                coupon.DataScadenza);
        });

        return Result<int>.Success(assegnazione.Id, "Coupon assegnato con successo.");
    }
}
