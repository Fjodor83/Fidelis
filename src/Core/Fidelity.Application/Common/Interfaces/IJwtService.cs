using Fidelity.Domain.Entities;

namespace Fidelity.Application.Common.Interfaces;

public interface IJwtService
{
    Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(int userId, string role, CancellationToken cancellationToken);
    Task<(bool IsValid, int? UserId, string? Role)> ValidateRefreshTokenAsync(string refreshToken, CancellationToken cancellationToken);
}
