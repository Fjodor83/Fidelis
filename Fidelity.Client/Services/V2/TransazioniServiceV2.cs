using Fidelity.Application.DTOs;
using System.Net.Http.Json;

namespace Fidelity.Client.Services.V2;

public class TransazioniServiceV2
{
    private readonly HttpClient _httpClient;

    public TransazioniServiceV2(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<int?> RegistraTransazioneAsync(RegistraTransazioneRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/transazioniv2", request);
        
        if (!response.IsSuccessStatusCode)
            return null;

        var result = await response.Content.ReadFromJsonAsync<RegistraTransazioneResponse>();
        return result?.TransazioneId;
    }

    public async Task<List<TransazioneDto>?> GetTransazioniClienteAsync(int clienteId)
    {
        return await _httpClient.GetFromJsonAsync<List<TransazioneDto>>($"api/transazioniv2/cliente/{clienteId}");
    }
}

public class RegistraTransazioneRequest
{
    public int ClienteId { get; set; }
    public int PuntoVenditaId { get; set; }
    public int? ResponsabileId { get; set; }
    public decimal Importo { get; set; }
    public string? Note { get; set; }
}

public class RegistraTransazioneResponse
{
    public int TransazioneId { get; set; }
    public bool Success { get; set; }
}
