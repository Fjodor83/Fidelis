using Fidelity.Shared.DTOs;

namespace Fidelity.Server.Services
{
    public interface IAnalyticsService
    {
        Task<DashboardStatsDTO> GetStatsAsync();
        Task<List<RegistrationStatsDTO>> GetRegistrationHistoryAsync();
        Task<List<RecentActivityDTO>> GetRecentActivityAsync();
    }
}
