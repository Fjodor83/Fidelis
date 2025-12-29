using Fidelity.Application.Common.Models;
using Fidelity.Application.DTOs;
using MediatR;

namespace Fidelity.Application.Clienti.Queries.GetCliente;

public record GetClienteQuery : IRequest<Result<ClienteDetailDto>>
{
    public int ClienteId { get; init; }
}

public record GetClienteByCodiceFidelityQuery : IRequest<Result<ClienteDetailDto>>
{
    public string CodiceFidelity { get; init; } = string.Empty;
}
