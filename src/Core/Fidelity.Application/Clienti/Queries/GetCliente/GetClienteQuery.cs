using Fidelity.Application.DTOs;
using MediatR;

namespace Fidelity.Application.Clienti.Queries.GetCliente;

public record GetClienteQuery(int Id) : IRequest<ClienteDto?>;
