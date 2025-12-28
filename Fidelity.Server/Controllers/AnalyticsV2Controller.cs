using Fidelity.Application.Analytics.Queries.GetDashboardStats;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fidelity.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin,Responsabile")]
public class AnalyticsV2Controller : ControllerBase
{
    private readonly IMediator _mediator;

    public AnalyticsV2Controller(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Get dashboard statistics
    /// </summary>
    [HttpGet("dashboard")]
    public async Task<ActionResult<DashboardStatsDto>> GetDashboardStats()
    {
        var stats = await _mediator.Send(new GetDashboardStatsQuery());
        return Ok(stats);
    }
}
