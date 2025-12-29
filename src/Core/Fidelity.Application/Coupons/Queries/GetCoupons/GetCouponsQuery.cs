using Fidelity.Application.DTOs;
using MediatR;

namespace Fidelity.Application.Coupons.Queries.GetCoupons;

public record GetCouponsQuery : IRequest<List<CouponDto>>;

public record GetCouponsDisponibiliQuery : IRequest<List<CouponDto>>;
