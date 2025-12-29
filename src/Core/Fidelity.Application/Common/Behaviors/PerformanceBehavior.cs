using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;
using Fidelity.Application.Common.Interfaces;

namespace Fidelity.Application.Common.Behaviors;

/// <summary>
/// Performance logging behavior - ISO 25000: Performance Efficiency
/// </summary>
public class PerformanceBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : notnull
{
    private readonly Stopwatch _timer;
    private readonly ILogger<TRequest> _logger;
    private readonly ICurrentUserService _currentUserService;

    public PerformanceBehavior(
        ILogger<TRequest> logger,
        ICurrentUserService currentUserService)
    {
        _timer = new Stopwatch();
        _logger = logger;
        _currentUserService = currentUserService;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        _timer.Start();

        var response = await next();

        _timer.Stop();

        var elapsedMilliseconds = _timer.ElapsedMilliseconds;

        if (elapsedMilliseconds > 500) // Log slow requests (>500ms)
        {
            var requestName = typeof(TRequest).Name;
            var userId = _currentUserService.UserId;
            var userName = _currentUserService.Username ?? "Anonymous";

            _logger.LogWarning(
                "Long Running Request: {Name} ({ElapsedMilliseconds} ms) UserId: {UserId} UserName: {UserName} Request: {@Request}",
                requestName, elapsedMilliseconds, userId, userName, request);
        }

        return response;
    }
}
