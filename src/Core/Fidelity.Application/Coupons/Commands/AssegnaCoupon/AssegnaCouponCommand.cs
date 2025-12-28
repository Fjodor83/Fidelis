using Fidelity.Application.Common.Models;
using MediatR;

namespace Fidelity.Application.Coupons.Commands.AssegnaCoupon;

public record AssegnaCouponCommand : IRequest<Result<int>>
{
    public int CouponId { get; init; }
    public int ClienteId { get; init; }
}
