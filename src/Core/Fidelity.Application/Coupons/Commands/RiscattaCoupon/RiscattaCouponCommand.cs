using Fidelity.Application.Common.Models;
using MediatR;

namespace Fidelity.Application.Coupons.Commands.RiscattaCoupon;

/// <summary>
/// Command to redeem a coupon - includes audit tracking
/// </summary>
public record RiscattaCouponCommand : IRequest<Result>
{
    public int CouponAssegnatoId { get; init; }
    public int ResponsabileId { get; init; }
    public int PuntoVenditaId { get; init; }
    public decimal? ImportoTransazione { get; init; }
}
