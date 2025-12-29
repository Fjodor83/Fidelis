using Fidelity.Application.Common.Interfaces;
using Fidelity.Application.Common.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fidelity.Application.Coupons.Commands.RiscattaCoupon;

public class RiscattaCouponCommandHandler : IRequestHandler<RiscattaCouponCommand, Result>
{
    private readonly IApplicationDbContext _context;
    private readonly ILogger<RiscattaCouponCommandHandler> _logger;

    public RiscattaCouponCommandHandler(
        IApplicationDbContext context,
        ILogger<RiscattaCouponCommandHandler> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Result> Handle(RiscattaCouponCommand request, CancellationToken cancellationToken)
    {
        var assegnazione = await _context.CouponAssegnati
            .Include(ca => ca.Coupon)
            .Include(ca => ca.Cliente)
            .FirstOrDefaultAsync(ca => ca.Id == request.CouponAssegnatoId, cancellationToken);

        if (assegnazione == null)
            return Result.Failure("Coupon assegnato non trovato.");

        if (assegnazione.Utilizzato)
            return Result.Failure("Coupon già utilizzato.");

        if (!assegnazione.Coupon.IsValido())
            return Result.Failure("Coupon scaduto o non più valido.");

        // Check minimum order amount if applicable
        if (assegnazione.Coupon.ImportoMinimoOrdine.HasValue &&
            request.ImportoTransazione.HasValue &&
            request.ImportoTransazione.Value < assegnazione.Coupon.ImportoMinimoOrdine.Value)
        {
            return Result.Failure($"Importo minimo ordine non raggiunto (€{assegnazione.Coupon.ImportoMinimoOrdine.Value}).");
        }

        // Mark coupon as used with audit info
        assegnazione.Utilizza(request.ResponsabileId, request.PuntoVenditaId);

        // Increment global usage counter
        assegnazione.Coupon.IncrementaUtilizzi();

        await _context.SaveChangesAsync(cancellationToken);

        _logger.LogInformation(
            "Coupon {CouponId} riscattato da cliente {ClienteId} presso PV {PuntoVenditaId} da responsabile {ResponsabileId}",
            assegnazione.CouponId, assegnazione.ClienteId, request.PuntoVenditaId, request.ResponsabileId);

        return Result.Success("Coupon riscattato con successo.");
    }
}
