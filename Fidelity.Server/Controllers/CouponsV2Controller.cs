using Fidelity.Application.Coupons.Commands.CreaCoupon;
using Fidelity.Application.Coupons.Commands.AssegnaCoupon;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fidelity.Server.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CouponsV2Controller : ControllerBase
{
    private readonly IMediator _mediator;

    public CouponsV2Controller(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Crea nuovo coupon
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<int>> CreaCoupon(CreaCouponCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { couponId = result.Data, success = true });
    }

    /// <summary>
    /// Assegna coupon a cliente
    /// </summary>
    [HttpPost("assegna")]
    [Authorize(Roles = "Admin,Responsabile")]
    public async Task<ActionResult<int>> AssegnaCoupon(AssegnaCouponCommand command)
    {
        var result = await _mediator.Send(command);

        if (!result.Succeeded)
            return BadRequest(new { errors = result.Errors });

        return Ok(new { couponAssegnatoId = result.Data, success = true });
    }
}
