using Fidelity.Application.DTOs;
using MediatR;

namespace Fidelity.Application.Transazioni.Queries.GetStoricoTransazioni;

public record GetStoricoTransazioniQuery : IRequest<List<TransazioneDto>>
{
    public int? PuntoVenditaId { get; init; }
    public int? ClienteId { get; init; }
    public DateTime? DataInizio { get; init; }
    public DateTime? DataFine { get; init; }
    public int Limit { get; init; } = 50;
}
