using Fidelity.Application.DTOs;
using MediatR;

namespace Fidelity.Application.Analytics.Queries.GetDashboardStats;

public record GetDashboardStatsQuery : IRequest<DashboardStatsDto>;
