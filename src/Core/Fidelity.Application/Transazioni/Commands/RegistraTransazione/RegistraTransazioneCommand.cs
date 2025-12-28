using Fidelity.Application.Common.Models;
using MediatR;

namespace Fidelity.Application.Transazioni.Commands.RegistraTransazione;

public record RegistraTransazioneCommand : IRequest<Result<int>>
{
    public int ClienteId { get; init; }
    public int PuntoVenditaId { get; init; }
    public int? ResponsabileId { get; init; }
    public decimal Importo { get; init; }
    public string? Note { get; init; }
}
