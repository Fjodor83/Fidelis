using Fidelity.Application.DTOs;
using MediatR;

namespace Fidelity.Application.Transazioni.Queries.GetTransazioniCliente;

public record GetTransazioniClienteQuery(int ClienteId) : IRequest<List<TransazioneDto>>;
