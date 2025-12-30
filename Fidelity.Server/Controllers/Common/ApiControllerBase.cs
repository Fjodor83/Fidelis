using Fidelity.Application.Common.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Fidelity.Server.Controllers.Common;

[ApiController]
[Produces("application/json")]
[ProducesResponseType(StatusCodes.Status500InternalServerError)]
public abstract class ApiControllerBase : ControllerBase
{
    private ISender? _mediator;
    protected ISender Mediator => _mediator ??= HttpContext.RequestServices.GetRequiredService<ISender>();
    
    protected ActionResult<T> HandleResult<T>(Result<T> result)
    {
        if (result.Succeeded)
            return Ok(result.Data);
        
        // Gestione errori intelligente
        var firstError = result.Errors.FirstOrDefault() ?? "Unknown error";
        
        return firstError switch
        {
            var e when e.Contains("non trovato", StringComparison.OrdinalIgnoreCase) 
                => NotFound(new { errors = result.Errors }),
            var e when e.Contains("non autorizzato", StringComparison.OrdinalIgnoreCase) 
                => Unauthorized(new { errors = result.Errors }),
            var e when e.Contains("già esiste", StringComparison.OrdinalIgnoreCase) 
                => Conflict(new { errors = result.Errors }),
            _ => BadRequest(new { errors = result.Errors })
        };
    }
    
    protected ActionResult HandleResult(Result result)
    {
        if (result.Succeeded)
            return Ok(new { message = result.Message ?? "Operation successful" });
        
        var firstError = result.Errors.FirstOrDefault() ?? "Unknown error";
        
        return firstError switch
        {
            var e when e.Contains("non trovato", StringComparison.OrdinalIgnoreCase) 
                => NotFound(new { errors = result.Errors }),
            _ => BadRequest(new { errors = result.Errors })
        };
    }
}
