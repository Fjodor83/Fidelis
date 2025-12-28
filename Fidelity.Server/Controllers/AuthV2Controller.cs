using Fidelity.Application.Auth.Commands.Login;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fidelity.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthV2Controller : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthV2Controller(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Login cliente
    /// </summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login(LoginClienteCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
            return Unauthorized(new { success = false, errors = result.Errors });

        return Ok(result.Data);
    }
}
