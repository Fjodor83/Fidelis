using Fidelity.Application.Transazioni.Commands.RegistraTransazione;
using Fidelity.Application.Transazioni.Queries.GetTransazioniCliente;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fidelity.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TransazioniV2Controller : ControllerBase
{
    private readonly IMediator _mediator;

    public TransazioniV2Controller(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Registra nuova transazione
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin,Responsabile")]
    public async Task<ActionResult<int>> RegistraTransazione(RegistraTransazioneCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { transazioneId = result.Data, success = true });
    }

    /// <summary>
    /// Ottieni transazioni di un cliente
    /// </summary>
    [HttpGet("cliente/{clienteId}")]
    public async Task<ActionResult<List<TransazioneDto>>> GetByCliente(int clienteId)
    {
        var transazioni = await _mediator.Send(new GetTransazioniClienteQuery(clienteId));
        return Ok(transazioni);
    }
}
