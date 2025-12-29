using Fidelity.Application.DTOs;
using MediatR;

namespace Fidelity.Application.Coupons.Queries.GetCouponsByCliente;

public record GetCouponsByClienteQuery(int ClienteId) : IRequest<List<CouponAssegnatoDto>>;
