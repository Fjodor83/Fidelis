using System.Security.Claims;

namespace Fidelity.Client.State;

public class AuthState
{
    public ClaimsPrincipal User { get; private set; } = new(new ClaimsIdentity());
    public bool IsAuthenticated => User.Identity?.IsAuthenticated ?? false;

    public void SetUser(ClaimsPrincipal user)
    {
        User = user;
    }
}
