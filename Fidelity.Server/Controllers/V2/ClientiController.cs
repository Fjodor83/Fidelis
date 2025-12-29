using Fidelity.Application.Clienti.Commands.RegistraCliente;
using Fidelity.Application.Clienti.Queries.GetCliente;
using Fidelity.Application.Common.Models;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fidelity.Server.Controllers.V2;

/// <summary>
/// Clienti Controller V2 - Clean Architecture
/// ISO 25000: Maintainability, Testability
/// </summary>
[ApiController]
[Route("api/v2/[controller]")]
public class ClientiController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<ClientiController> _logger;

    public ClientiController(IMediator mediator, ILogger<ClientiController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Registra un nuovo cliente (self-service)
    /// </summary>
    [HttpPost("registra")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(Result<RegistraClienteResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Result), StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<RegistraClienteResponse>>> Registra(
        [FromBody] RegistraClienteCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        _logger.LogInformation("Cliente registrato: {ClienteId}", result.Data?.ClienteId);
        return Ok(result);
    }

    /// <summary>
    /// Ottiene i dettagli di un cliente per ID
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(Result<ClienteDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<ClienteDetailDto>>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(new GetClienteQuery { ClienteId = id }, cancellationToken);

        if (!result.Succeeded)
        {
            return NotFound(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Ottiene i dettagli di un cliente per codice fidelity
    /// </summary>
    [HttpGet("codice/{codiceFidelity}")]
    [Authorize(Roles = "Admin,Responsabile")]
    [ProducesResponseType(typeof(Result<ClienteDetailDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Result<ClienteDetailDto>>> GetByCodiceFidelity(
        string codiceFidelity,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(
            new GetClienteByCodiceFidelityQuery { CodiceFidelity = codiceFidelity },
            cancellationToken);

        if (!result.Succeeded)
        {
            return NotFound(result);
        }

        return Ok(result);
    }
}
