using Fidelity.Application.Common.Models;
using MediatR;

namespace Fidelity.Application.Clienti.Commands.RegistraCliente;

/// <summary>
/// Command to register a new customer
/// </summary>
public record RegistraClienteCommand : IRequest<Result<RegistraClienteResponse>>
{
    public string Nome { get; init; } = string.Empty;
    public string Cognome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Telefono { get; init; }
    public string Password { get; init; } = string.Empty;
    public int? PuntoVenditaId { get; init; }
    public bool PrivacyAccepted { get; init; }
    public bool HasExistingCard { get; init; }
    public string? ExistingFidelityCode { get; init; }
}

public record RegistraClienteResponse
{
    public int ClienteId { get; init; }
    public string CodiceFidelity { get; init; } = string.Empty;
    public string Nome { get; init; } = string.Empty;
    public string Cognome { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string Token { get; init; } = string.Empty;
    public string? RefreshToken { get; init; }
    public int PuntiTotali { get; init; }
    public string? Message { get; init; }
}
