using System.Security.Claims;
using Fidelity.Application.Common.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Fidelity.Infrastructure.Services;

/// <summary>
/// Current User Service - extracts user info from JWT claims
/// ISO 25000: Security, Traceability
/// </summary>
public class CurrentUserService : ICurrentUserService
{
    private readonly IHttpContextAccessor _httpContextAccessor;

    public CurrentUserService(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    private ClaimsPrincipal? User => _httpContextAccessor.HttpContext?.User;

    public int? UserId
    {
        get
        {
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(userIdClaim, out var id) ? id : null;
        }
    }

    public string? Username => User?.FindFirst(ClaimTypes.Name)?.Value;

    public string? Ruolo => User?.FindFirst(ClaimTypes.Role)?.Value;

    public int? PuntoVenditaId
    {
        get
        {
            var pvIdClaim = User?.FindFirst("PuntoVenditaId")?.Value;
            return int.TryParse(pvIdClaim, out var id) ? id : null;
        }
    }

    public bool IsAuthenticated => User?.Identity?.IsAuthenticated ?? false;

    public bool IsAdmin => Ruolo == "Admin";

    public bool IsResponsabile => Ruolo == "Responsabile" || IsAdmin;

    public bool IsCliente => Ruolo == "Cliente";
}
