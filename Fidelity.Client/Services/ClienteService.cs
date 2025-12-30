using Fidelity.Client.Services.Interfaces;
using Fidelity.Application.DTOs; // Changed from Shared to Application
using System.Net.Http.Json;

namespace Fidelity.Client.Services;

public class ClienteService : IClienteService
{
    private readonly HttpClient _httpClient;

    public ClienteService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ClienteDetailDto> GetByCodiceFidelity(string codice)
    {
        return await _httpClient.GetFromJsonAsync<ClienteDetailDto>($"api/v2/clienti/codice/{codice}") 
               ?? throw new Exception("Cliente non trovato");
    }

    public async Task<ClienteDetailDto> GetById(int id)
    {
         return await _httpClient.GetFromJsonAsync<ClienteDetailDto>($"api/v2/clienti/{id}")
               ?? throw new Exception("Cliente non trovato");
    }

    public async Task<List<ClienteDto>> Search(string query)
    {
        return await _httpClient.GetFromJsonAsync<List<ClienteDto>>($"api/v2/clienti/search?query={query}")
               ?? new List<ClienteDto>();
    }

    public async Task<ClienteDetailDto> GetClienteDettaglioAsync(string clienteId)
    {
        if (int.TryParse(clienteId, out int id))
        {
             return await GetById(id);
        }
        // If code, lookup by code
        else if (clienteId.Length >= 5) // Simple heuristic
        {
             return await GetByCodiceFidelity(clienteId);
        }
        throw new ArgumentException("Invalid ID format");
    }
}
