using Fidelity.Application.Common.Models;
using MediatR;

namespace Fidelity.Application.Coupons.Commands.CreaCoupon;

public record CreaCouponCommand : IRequest<Result<int>>
{
    public string Codice { get; init; } = string.Empty;
    public string Titolo { get; init; } = string.Empty;
    public string? Descrizione { get; init; }
    public decimal ValoreSconto { get; init; }
    public string TipoSconto { get; init; } = "Percentuale"; // "Percentuale" o "Fisso"
    public DateTime DataInizio { get; init; }
    public DateTime DataScadenza { get; init; }
    public bool Attivo { get; init; } = true;
}
