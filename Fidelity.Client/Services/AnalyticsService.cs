using System.Net.Http.Json;
using Fidelity.Shared.DTOs;
using Microsoft.Extensions.Caching.Memory;
using Fidelity.Client.Services.Interfaces;

namespace Fidelity.Client.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;
    private readonly ILogger<AnalyticsService> _logger;

    public AnalyticsService(
        HttpClient http,
        IMemoryCache cache,
        ILogger<AnalyticsService> logger)
    {
        _http = http;
        _cache = cache;
        _logger = logger;
    }

    public async Task<DashboardStatsDTO> GetStatsAsync()
    {
        const string cacheKey = "dashboard_stats";

        if (_cache.TryGetValue(cacheKey, out DashboardStatsDTO? cached))
            return cached!;

        try
        {
            var result = await _http.GetFromJsonAsync<DashboardStatsDTO>("api/Analytics/stats")
                ?? new DashboardStatsDTO();

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching dashboard stats");
            return new DashboardStatsDTO();
        }
    }

    public async Task<List<RegistrationStatsDTO>> GetRegistrationHistoryAsync()
    {
        const string cacheKey = "registration_history";

        if (_cache.TryGetValue(cacheKey, out List<RegistrationStatsDTO>? cached))
            return cached!;

        try
        {
            var result = await _http.GetFromJsonAsync<List<RegistrationStatsDTO>>("api/Analytics/registrations-history")
                ?? new List<RegistrationStatsDTO>();

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(10));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching registration history");
            return new List<RegistrationStatsDTO>();
        }
    }

    public async Task<List<RecentActivityDTO>> GetRecentActivityAsync()
    {
        try
        {
            var result = await _http.GetFromJsonAsync<List<RecentActivityDTO>>("api/Analytics/recent-activity")
                ?? new List<RecentActivityDTO>();

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching recent activity");
            return new List<RecentActivityDTO>();
        }
    }
}