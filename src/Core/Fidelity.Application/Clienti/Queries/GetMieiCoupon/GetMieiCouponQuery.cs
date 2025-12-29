using Fidelity.Application.Common.Models;
using Fidelity.Application.DTOs;
using MediatR;

namespace Fidelity.Application.Coupons.Queries.GetMieiCoupon;

public record GetMieiCouponQuery : IRequest<Result<List<CouponAssegnatoDto>>>
{
    public int ClienteId { get; init; }
    public bool SoloAttivi { get; init; } = false;
}
