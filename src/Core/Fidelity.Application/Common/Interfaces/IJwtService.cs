using Fidelity.Domain.Entities;

namespace Fidelity.Application.Common.Interfaces;

/// <summary>
/// JWT service interface - ISO 25000: Security
/// </summary>
public interface IJwtService
{
    (string Token, string JwtId) GenerateToken(Cliente cliente);
    (string Token, string JwtId) GenerateToken(Responsabile responsabile, int? puntoVenditaId = null);
    Task<(string Token, string RefreshToken)> GenerateTokensAsync(int userId, string role, CancellationToken cancellationToken = default);
    bool ValidateToken(string token);
    string? GetClaimValue(string token, string claimType);
}
