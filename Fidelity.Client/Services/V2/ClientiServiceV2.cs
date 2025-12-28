using Fidelity.Application.DTOs;
using System.Net.Http.Json;

namespace Fidelity.Client.Services.V2;

public class ClientiServiceV2
{
    private readonly HttpClient _httpClient;

    public ClientiServiceV2(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ClienteDto?> GetClienteAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<ClienteDto>($"api/clientiv2/{id}");
    }

    public async Task<List<ClienteDto>?> GetClientiAsync(bool? soloAttivi = null, string? search = null)
    {
        var query = $"api/clientiv2?soloAttivi={soloAttivi}&search={search}";
        return await _httpClient.GetFromJsonAsync<List<ClienteDto>>(query);
    }

    public async Task<int?> RegistraAsync(RegistraClienteRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/clientiv2/registra", request);
        
        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<RegistraClienteResponse>();
        return result?.ClienteId;
    }
}

public class RegistraClienteRequest
{
    public string Nome { get; set; } = string.Empty;
    public string Cognome { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Telefono { get; set; }
    public string Password { get; set; } = string.Empty;
    public int? PuntoVenditaId { get; set; }
    public bool PrivacyAccepted { get; set; }
}

public class RegistraClienteResponse
{
    public int ClienteId { get; set; }
    public bool Success { get; set; }
}
