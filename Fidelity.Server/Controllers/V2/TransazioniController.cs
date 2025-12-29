using Fidelity.Application.Transazioni.Commands.AssegnaPunti;
using Fidelity.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fidelity.Server.Controllers.V2;

/// <summary>
/// Transazioni Controller V2 - Clean Architecture
/// ISO 25000: Traceability, Reliability
/// </summary>
[ApiController]
[Route("api/v2/[controller]")]
[Authorize(Roles = "Admin,Responsabile")]
public class TransazioniController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<TransazioniController> _logger;

    public TransazioniController(IMediator mediator, ILogger<TransazioniController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Assegna punti a un cliente per un acquisto
    /// </summary>
    [HttpPost("assegna-punti")]
    [ProducesResponseType(typeof(Result<AssegnaPuntiResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<AssegnaPuntiResponse>>> AssegnaPunti(
        [FromBody] AssegnaPuntiRequest request,
        CancellationToken cancellationToken)
    {
        var responsabileId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var puntoVenditaId = int.Parse(User.FindFirst("PuntoVenditaId")?.Value ?? "0");

        var command = new AssegnaPuntiCommand
        {
            ClienteId = request.ClienteId,
            ImportoSpesa = request.ImportoSpesa,
            PuntoVenditaId = puntoVenditaId,
            ResponsabileId = responsabileId,
            Note = request.Note
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        _logger.LogInformation(
            "Punti assegnati: {Punti} a cliente {ClienteId}",
            result.Data?.PuntiAssegnati, request.ClienteId);

        return Ok(result);
    }
}

public record AssegnaPuntiRequest
{
    public int ClienteId { get; init; }
    public decimal ImportoSpesa { get; init; }
    public string? Note { get; init; }
}
