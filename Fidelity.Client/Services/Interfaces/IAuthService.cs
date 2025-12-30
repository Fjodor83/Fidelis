using Fidelity.Shared.DTOs;

namespace Fidelity.Client.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<LoginClienteResponse?> LoginClienteAsync(LoginClienteRequest request);
    Task<LoginResponse?> RefreshTokenAsync();
    Task LogoutAsync();

    Task<bool> IsAuthenticatedAsync();
    Task<string?> GetTokenAsync();
    Task<string?> GetRoleAsync();
    Task<int?> GetUserIdAsync();

    event Action? OnAuthStateChanged;
}