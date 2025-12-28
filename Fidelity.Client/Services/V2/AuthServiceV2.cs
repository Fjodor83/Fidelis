using Fidelity.Application.DTOs;
using System.Net.Http.Json;

namespace Fidelity.Client.Services.V2;

public class AuthServiceV2
{
    private readonly HttpClient _httpClient;

    public AuthServiceV2(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<LoginResponseDto?> LoginAsync(string emailOrCode, string password)
    {
        var request = new
        {
            EmailOrCode = emailOrCode,
            Password = password
        };

        var response = await _httpClient.PostAsJsonAsync("api/authv2/login", request);

        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<LoginResponseDto>();
    }
}
