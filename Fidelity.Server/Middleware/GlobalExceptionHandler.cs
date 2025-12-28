using Microsoft.AspNetCore.Diagnostics;

namespace Fidelity.Server.Middleware;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    {
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext, 
        Exception exception, 
        CancellationToken cancellationToken)
    {
        var (statusCode, message) = exception switch
        {
            KeyNotFoundException => (StatusCodes.Status404NotFound, exception.Message),
            ArgumentException => (StatusCodes.Status400BadRequest, exception.Message),
            UnauthorizedAccessException => (StatusCodes.Status401Unauthorized, "Accesso non autorizzato"),
            InvalidOperationException => (StatusCodes.Status409Conflict, exception.Message),
            _ => (StatusCodes.Status500InternalServerError, "Si è verificato un errore interno")
        };

        _logger.LogError(exception, "Exception occurred: {Message}", exception.Message);

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(new 
        { 
            messaggio = message,
            statusCode 
        }, cancellationToken);

        return true;
    }
}
