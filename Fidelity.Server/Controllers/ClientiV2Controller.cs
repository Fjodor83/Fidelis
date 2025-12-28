using Fidelity.Application.Clienti.Commands.RegistraCliente;
using Fidelity.Application.Clienti.Queries.GetCliente;
using Fidelity.Application.Clienti.Queries.GetClienti;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fidelity.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ClientiV2Controller : ControllerBase
{
    private readonly IMediator _mediator;

    public ClientiV2Controller(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Registra un nuovo cliente
    /// </summary>
    [HttpPost("registra")]
    [AllowAnonymous]
    public async Task<ActionResult<int>> Registra(RegistraClienteCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { clienteId = result.Data, success = true });
    }

    /// <summary>
    /// Ottieni cliente per ID
    /// </summary>
    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<ClienteDto>> Get(int id)
    {
        var cliente = await _mediator.Send(new GetClienteQuery(id));

        if (cliente == null)
            return NotFound();

        return Ok(cliente);
    }

    /// <summary>
    /// Ottieni lista clienti con filtri
    /// </summary>
    [HttpGet]
    [Authorize(Roles = "Admin,Responsabile")]
    public async Task<ActionResult<List<ClienteDto>>> GetAll([FromQuery] bool? soloAttivi, [FromQuery] string? search)
    {
        var query = new GetClientiQuery
        {
            SoloAttivi = soloAttivi,
            SearchTerm = search
        };

        var clienti = await _mediator.Send(query);

        return Ok(clienti);
    }
}
