using Fidelity.Application.Common.Models;
using Fidelity.Application.DTOs;
using MediatR;

namespace Fidelity.Application.Coupons.Commands.UpdateCoupon;

public record UpdateCouponCommand : IRequest<Result<CouponDto>>
{
    public int Id { get; init; }
    public string Codice { get; init; } = string.Empty;
    public string Titolo { get; init; } = string.Empty;
    public string? Descrizione { get; init; }
    public decimal ValoreSconto { get; init; }
    public string TipoSconto { get; init; } = "Percentuale";
    public DateTime DataInizio { get; init; }
    public DateTime DataScadenza { get; init; }
    public bool Attivo { get; init; }
    public int? LimiteUtilizzoGlobale { get; init; }
    public decimal? ImportoMinimoOrdine { get; init; }
}
