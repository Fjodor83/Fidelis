using Fidelity.Client.Services.Interfaces;
using Fidelity.Shared.DTOs;
using System.Net.Http.Json;

namespace Fidelity.Client.Services;

public class ResponsabileService : IResponsabileService
{
    private readonly HttpClient _httpClient;

    public ResponsabileService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> CambiaPasswordAsync(CambiaPasswordRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/Responsabili/cambia-password", request);
        return response.IsSuccessStatusCode;
    }
}
