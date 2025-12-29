using Fidelity.Application.Common.Models;
using Fidelity.Application.DTOs;
using MediatR;

namespace Fidelity.Application.Coupons.Queries.GetCouponById;

public record GetCouponByIdQuery(int Id) : IRequest<Result<CouponDto>>;
