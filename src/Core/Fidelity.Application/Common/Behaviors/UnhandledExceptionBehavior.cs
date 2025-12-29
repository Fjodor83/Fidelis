using MediatR;
using Microsoft.Extensions.Logging;

namespace Fidelity.Application.Common.Behaviors;

/// <summary>
/// Unhandled exception behavior - ISO 25000: Reliability
/// </summary>
public class UnhandledExceptionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly ILogger<TRequest> _logger;

    public UnhandledExceptionBehavior(ILogger<TRequest> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        try
        {
            return await next();
        }
        catch (Exception ex)
        {
            var requestName = typeof(TRequest).Name;

            _logger.LogError(ex,
                "Unhandled Exception for Request {Name}: {@Request}",
                requestName, request);

            throw;
        }
    }
}
