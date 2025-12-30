using Fidelity.Application.DTOs;
using System.Net.Http.Json;

namespace Fidelity.Client.Services.V2;

public class AnalyticsServiceV2
{
    private readonly HttpClient _httpClient;

    public AnalyticsServiceV2(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<DashboardStatsDto?> GetDashboardStatsAsync()
    {
        return await _httpClient.GetFromJsonAsync<DashboardStatsDto>("api/v2/analytics/dashboard");
    }
}
