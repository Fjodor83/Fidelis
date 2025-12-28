using Fidelity.Application.DTOs;
using MediatR;

namespace Fidelity.Application.Clienti.Queries.GetClienti;

public record GetClientiQuery : IRequest<List<ClienteDto>>
{
    public bool? SoloAttivi { get; init; }
    public string? SearchTerm { get; init; }
}
