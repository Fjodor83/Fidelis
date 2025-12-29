using MediatR;
using Microsoft.Extensions.Logging;
using Fidelity.Application.Common.Interfaces;
using System.Text.Json;

namespace Fidelity.Application.Common.Behaviors;

/// <summary>
/// Logging behavior for audit trail - ISO 25000: Traceability
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;
    private readonly ICurrentUserService _currentUserService;

    public LoggingBehavior(
        ILogger<LoggingBehavior<TRequest, TResponse>> logger,
        ICurrentUserService currentUserService)
    {
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        var requestName = typeof(TRequest).Name;
        var userId = _currentUserService.UserId;
        var userName = _currentUserService.Username ?? "Anonymous";

        _logger.LogInformation(
            "Handling {RequestName} by User {UserId} ({UserName})",
            requestName, userId, userName);

        // Don't log sensitive data
        if (!requestName.Contains("Login") && !requestName.Contains("Password"))
        {
            try
            {
                var requestJson = JsonSerializer.Serialize(request, new JsonSerializerOptions
                {
                    WriteIndented = false,
                    MaxDepth = 3
                });

                _logger.LogDebug("Request data: {RequestData}", requestJson);
            }
            catch
            {
                // Ignore serialization errors
            }
        }

        var response = await next();

        _logger.LogInformation(
            "Handled {RequestName} successfully",
            requestName);

        return response;
    }
}
