using Fidelity.Application.Common.Models;
using MediatR;

namespace Fidelity.Application.Coupons.Commands.DeleteCoupon;

public record DeleteCouponCommand(int Id) : IRequest<Result>;
