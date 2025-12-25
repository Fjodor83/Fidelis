using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Fidelity.Shared.DTOs;
using Fidelity.Server.Services;

namespace Fidelity.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        [HttpGet("stats")]
        public async Task<ActionResult<DashboardStatsDTO>> GetStats()
        {
            var stats = await _analyticsService.GetStatsAsync();
            return Ok(stats);
        }

        [HttpGet("registrations-history")]
        public async Task<ActionResult<List<RegistrationStatsDTO>>> GetRegistrationHistory()
        {
            var history = await _analyticsService.GetRegistrationHistoryAsync();
            return Ok(history);
        }

        [HttpGet("recent-activity")]
        public async Task<ActionResult<List<RecentActivityDTO>>> GetRecentActivity()
        {
            var activity = await _analyticsService.GetRecentActivityAsync();
            return Ok(activity);
        }
    }
}
