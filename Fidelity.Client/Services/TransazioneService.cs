using Fidelity.Client.Services.Interfaces;
using Fidelity.Application.DTOs;
using Fidelity.Shared.DTOs;
using System.Net.Http.Json;

namespace Fidelity.Client.Services;

public class TransazioneService : ITransazioneService
{
    private readonly HttpClient _httpClient;

    public TransazioneService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<bool> RegistraTransazione(TransazioneRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v2/transazioni", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<TransazioneDto?> AssegnaPuntiAsync(AssegnaPuntiRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/v2/transazioni/assegna", request);
        if (response.IsSuccessStatusCode)
        {
            try 
            {
               return await response.Content.ReadFromJsonAsync<TransazioneDto>();
            }
            catch
            {
                // If returns simpler object or just Ok
                return null;
            }
        }
        return null;
    }

    public async Task<List<TransazioneDto>> GetUltimiMovimenti(int clienteId)
    {
        return await _httpClient.GetFromJsonAsync<List<TransazioneDto>>($"api/v2/transazioni/cliente/{clienteId}") 
               ?? new List<TransazioneDto>();
    }
}
