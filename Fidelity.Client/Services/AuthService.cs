using Fidelity.Client.Services.Interfaces;
using Fidelity.Shared.DTOs;
using Fidelity.Client.State;
using Fidelity.Client.Helpers;
using System.Net.Http.Json;
using System.Net.Http.Headers;
using System.Security.Claims;
using Blazored.LocalStorage;

namespace Fidelity.Client.Services;

public class AuthService : IAuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthState _authState;
    private readonly StorageHelper _storageHelper;
    private readonly ILocalStorageService _localStorage;

    public event Action? OnAuthStateChanged;

    public AuthService(HttpClient httpClient, AuthState authState, StorageHelper storageHelper, ILocalStorageService localStorage)
    {
        _httpClient = httpClient;
        _authState = authState;
        _storageHelper = storageHelper;
        _localStorage = localStorage;
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);
        if (response.IsSuccessStatusCode)
        {
            var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
            if (result != null)
            {
                await _localStorage.SetItemAsync("authToken", result.Token);
                await _storageHelper.SaveRefreshTokenAsync(result.RefreshToken);
                
                // Update State
                var claims = JwtParser.ParseClaimsFromJwt(result.Token);
                _authState.SetUser(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
                
                NotifyAuthStateChanged();
                return result;
            }
        }
        return null;
    }

    public async Task<LoginClienteResponse?> LoginClienteAsync(LoginClienteRequest request)
    {
         var response = await _httpClient.PostAsJsonAsync("api/v2/auth/login-cliente", request);
         if (response.IsSuccessStatusCode)
         {
             var result = await response.Content.ReadFromJsonAsync<LoginClienteResponse>();
             if (result != null)
             {
                 await _localStorage.SetItemAsync("authToken", result.Token);
                 // Assuming RefreshToken is also here or handled same way
                  var claims = JwtParser.ParseClaimsFromJwt(result.Token);
                 _authState.SetUser(new ClaimsPrincipal(new ClaimsIdentity(claims, "jwt")));
                 
                 NotifyAuthStateChanged();
                 return result;
             }
         }
         return null;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        _authState.SetUser(new ClaimsPrincipal(new ClaimsIdentity()));
        NotifyAuthStateChanged();
    }

    public async Task<LoginResponse?> RefreshTokenAsync()
    {
        // TODO: Implement refresh logic using stored refresh token
        return null;
    }

    public async Task<bool> IsAuthenticatedAsync()
    {
        var token = await _localStorage.GetItemAsync<string>("authToken");
        if (string.IsNullOrEmpty(token)) return false;
        
        // Basic check, could verify expiration
        return true;
    }

    public async Task<string?> GetTokenAsync()
    {
        return await _localStorage.GetItemAsync<string>("authToken");
    }

    public async Task<string?> GetRoleAsync()
    {
       if (!_authState.IsAuthenticated) return null;
       return _authState.User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role)?.Value;
    }

    public async Task<int?> GetUserIdAsync()
    {
         if (!_authState.IsAuthenticated) return null;
         var idClaim = _authState.User.Claims.FirstOrDefault(c => c.Type == "id" || c.Type == ClaimTypes.NameIdentifier);
         if (idClaim != null && int.TryParse(idClaim.Value, out int id))
            return id;
         return null;
    }
    
    private void NotifyAuthStateChanged() => OnAuthStateChanged?.Invoke();
}
