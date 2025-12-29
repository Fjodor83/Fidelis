using Fidelity.Application.Coupons.Commands.AssegnaCoupon;
using Fidelity.Application.Coupons.Commands.CreaCoupon;
using Fidelity.Application.Coupons.Commands.RiscattaCoupon;
using Fidelity.Application.Coupons.Queries.GetMieiCoupon;
using Fidelity.Application.Common.Models;
using Fidelity.Application.DTOs;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fidelity.Server.Controllers.V2;

/// <summary>
/// Coupons Controller V2 - Clean Architecture
/// ISO 25000: Functional Suitability, Security
/// </summary>
[ApiController]
[Route("api/v2/[controller]")]
[Authorize]
public class CouponsController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly ILogger<CouponsController> _logger;

    public CouponsController(IMediator mediator, ILogger<CouponsController> logger)
    {
        _mediator = mediator;
        _logger = logger;
    }

    /// <summary>
    /// Crea un nuovo coupon (solo Admin)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<int>>> Create(
        [FromBody] CreaCouponCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Assegna un coupon a un cliente
    /// </summary>
    [HttpPost("assegna")]
    [Authorize(Roles = "Admin,Responsabile")]
    [ProducesResponseType(typeof(Result<int>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result<int>>> Assegna(
        [FromBody] AssegnaCouponCommand command,
        CancellationToken cancellationToken)
    {
        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Riscatta (utilizza) un coupon
    /// </summary>
    [HttpPost("riscatta")]
    [Authorize(Roles = "Admin,Responsabile")]
    [ProducesResponseType(typeof(Result), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Result>> Riscatta(
        [FromBody] RiscattaCouponRequest request,
        CancellationToken cancellationToken)
    {
        var responsabileId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var puntoVenditaId = int.Parse(User.FindFirst("PuntoVenditaId")?.Value ?? "0");

        var command = new RiscattaCouponCommand
        {
            CouponAssegnatoId = request.CouponAssegnatoId,
            ResponsabileId = responsabileId,
            PuntoVenditaId = puntoVenditaId,
            ImportoTransazione = request.ImportoTransazione
        };

        var result = await _mediator.Send(command, cancellationToken);

        if (!result.Succeeded)
        {
            return BadRequest(result);
        }

        return Ok(result);
    }

    /// <summary>
    /// Ottiene i coupon del cliente autenticato
    /// </summary>
    [HttpGet("miei")]
    [Authorize(Roles = "Cliente")]
    [ProducesResponseType(typeof(Result<List<CouponAssegnatoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<CouponAssegnatoDto>>>> GetMieiCoupon(
        [FromQuery] bool soloAttivi = false,
        CancellationToken cancellationToken = default)
    {
        var clienteId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var query = new GetMieiCouponQuery
        {
            ClienteId = clienteId,
            SoloAttivi = soloAttivi
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Ottiene i coupon di un cliente specifico (per responsabili)
    /// </summary>
    [HttpGet("cliente/{clienteId:int}")]
    [Authorize(Roles = "Admin,Responsabile")]
    [ProducesResponseType(typeof(Result<List<CouponAssegnatoDto>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<Result<List<CouponAssegnatoDto>>>> GetCouponCliente(
        int clienteId,
        [FromQuery] bool soloAttivi = false,
        CancellationToken cancellationToken = default)
    {
        var query = new GetMieiCouponQuery
        {
            ClienteId = clienteId,
            SoloAttivi = soloAttivi
        };

        var result = await _mediator.Send(query, cancellationToken);
        return Ok(result);
    }
}

// Request DTO for riscatta endpoint
public record RiscattaCouponRequest
{
    public int CouponAssegnatoId { get; init; }
    public decimal? ImportoTransazione { get; init; }
}
