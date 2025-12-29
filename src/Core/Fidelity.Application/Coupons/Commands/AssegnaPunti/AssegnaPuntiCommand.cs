using Fidelity.Application.Common.Models;
using MediatR;

namespace Fidelity.Application.Transazioni.Commands.AssegnaPunti;

/// <summary>
/// Command to assign points for a purchase
/// </summary>
public record AssegnaPuntiCommand : IRequest<Result<AssegnaPuntiResponse>>
{
    public int ClienteId { get; init; }
    public decimal ImportoSpesa { get; init; }
    public int PuntoVenditaId { get; init; }
    public int ResponsabileId { get; init; }
    public string? Note { get; init; }
}

public record AssegnaPuntiResponse
{
    public int TransazioneId { get; init; }
    public int PuntiAssegnati { get; init; }
    public int PuntiTotaliCliente { get; init; }
    public string? NuovoLivello { get; init; }
}
