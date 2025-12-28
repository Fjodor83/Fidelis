using Fidelity.Application.Common.Models;
using MediatR;

namespace Fidelity.Application.Clienti.Commands.RegistraCliente;

public record RegistraClienteCommand : IRequest<Result<int>>
{
    public string Nome { get; init; } = string.Empty;
    public string Cognome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Telefono { get; init; }
    public string Password { get; init; } = string.Empty;
    public int? PuntoVenditaId { get; init; }
    public bool PrivacyAccepted { get; init; }
}
