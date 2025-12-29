namespace Fidelity.Application.Common.Interfaces;

/// <summary>
/// Current user service for audit tracking - ISO 25000: Traceability
/// </summary>
public interface ICurrentUserService
{
    int? UserId { get; }
    string? Username { get; }
    string? Ruolo { get; }
    int? PuntoVenditaId { get; }
    bool IsAuthenticated { get; }
    bool IsAdmin { get; }
    bool IsResponsabile { get; }
    bool IsCliente { get; }
}
