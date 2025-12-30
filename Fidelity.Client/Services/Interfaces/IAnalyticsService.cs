using Fidelity.Shared.DTOs;

namespace Fidelity.Client.Services.Interfaces;

public interface IAnalyticsService
{
    Task<DashboardStatsDTO> GetStatsAsync();
    Task<List<RegistrationStatsDTO>> GetRegistrationHistoryAsync();
    Task<List<RecentActivityDTO>> GetRecentActivityAsync();
}
