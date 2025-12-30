
using Fidelity.Application.Clienti.Commands.RegistraCliente;
using Fidelity.Application.Clienti.Queries.GetCliente;
using Fidelity.Application.Common.Models;
using Fidelity.Application.DTOs;
using Fidelity.Server.Controllers.Common;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Asp.Versioning;

namespace Fidelity.Server.Controllers.V2;

/// <summary>
/// Clienti Controller V2 - Clean Architecture
/// ISO 25000: Maintainability, Testability
/// </summary>
[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class ClientiController : ApiControllerBase
{
    private readonly ILogger<ClientiController> _logger;
    private readonly Fidelity.Application.Common.Interfaces.ICardGeneratorService _cardGenerator;

    public ClientiController(ILogger<ClientiController> logger, Fidelity.Application.Common.Interfaces.ICardGeneratorService cardGenerator)
    {
        _logger = logger;
        _cardGenerator = cardGenerator;
    }

    /// <summary>
    /// Genera QR Code per codice fidelity
    /// </summary>
    [HttpGet("qrcode/{codiceFidelity}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(FileContentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetQrCode(string codiceFidelity, int dimensione = 200)
    {
        var qrBytes = await _cardGenerator.GeneraQRCodeAsync(codiceFidelity, dimensione);
        return File(qrBytes, "image/png");
    }

    /// <summary>
    /// Registra un nuovo cliente (self-service)
    /// </summary>
    [HttpPost("registra")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegistraClienteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<RegistraClienteResponse>> Registra(
        [FromBody] RegistraClienteCommand command,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(command, cancellationToken);

        if (result.Succeeded)
        {
            _logger.LogInformation("Cliente registrato: {ClienteId}", result.Data?.ClienteId);
        }

        return HandleResult(result);
    }

    /// <summary>
    /// Ottiene i dettagli di un cliente per ID
    /// </summary>
    [HttpGet("{id:int}")]
    [Authorize]
    [ProducesResponseType(typeof(ClienteDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDetailDto>> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(new GetClienteQuery { ClienteId = id }, cancellationToken);
        return HandleResult(result);
    }

    /// <summary>
    /// Ottiene i dettagli di un cliente per codice fidelity
    /// </summary>
    [HttpGet("codice/{codiceFidelity}")]
    [Authorize(Roles = "Admin,Responsabile")]
    [ProducesResponseType(typeof(ClienteDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ClienteDetailDto>> GetByCodiceFidelity(
        string codiceFidelity,
        CancellationToken cancellationToken)
    {
        var result = await Mediator.Send(
            new GetClienteByCodiceFidelityQuery { CodiceFidelity = codiceFidelity },
            cancellationToken);

        return HandleResult(result);
    }
}
