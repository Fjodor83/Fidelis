using Fidelity.Application.Common.Models;
using MediatR;

namespace Fidelity.Application.Coupons.Commands.AssegnaCoupon;

public record AssegnaCouponCommand : IRequest<Result<int>>
{
    public int CouponId { get; init; }
    public int ClienteId { get; init; }
    public MotivoAssegnazioneDto Motivo { get; init; } = MotivoAssegnazioneDto.Manuale;
}

public enum MotivoAssegnazioneDto
{
    Manuale = 0,
    Automatico = 1,
    Benvenuto = 2,
    Compleanno = 3,
    LivelloRaggiunto = 4,
    Promozione = 5
}
