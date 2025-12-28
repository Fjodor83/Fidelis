using System.Collections.Concurrent;

namespace Fidelity.Server.Middleware;

public class RateLimitingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private readonly RateLimitOptions _options;
    
    // Store: IP -> (WindowStart, RequestCount)
    private static readonly ConcurrentDictionary<string, (DateTime WindowStart, int Count)> _requestCounts = new();
    
    public RateLimitingMiddleware(
        RequestDelegate next,
        ILogger<RateLimitingMiddleware> logger,
        RateLimitOptions options)
    {
        _next = next;
        _logger = logger;
        _options = options;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Skip rate limiting for static files and framework resources
        var path = context.Request.Path.Value?.ToLowerInvariant();
        if (path != null && (
            path.StartsWith("/_framework/") ||
            path.StartsWith("/_content/") ||
            path.Contains(".wasm") ||
            path.Contains(".js") ||
            path.Contains(".css") ||
            path.Contains(".json") ||
            path.Contains(".woff") ||
            path.Contains(".woff2") ||
            path.Contains(".ttf") ||
            path.Contains(".svg") ||
            path.Contains(".png") ||
            path.Contains(".jpg") ||
            path.Contains(".gif") ||
            path.Contains(".ico")))
        {
            await _next(context);
            return;
        }
        
        // Only apply rate limiting to API endpoints
        if (path == null || !path.StartsWith("/api/"))
        {
            await _next(context);
            return;
        }

        var clientIp = GetClientIpAddress(context);
        var now = DateTime.UtcNow;
        
        // Clean up old entries (every 100 requests)
        if (_requestCounts.Count > 1000)
        {
            CleanupOldEntries(now);
        }
        
        // Get or create request tracking for this IP
        var (windowStart, count) = _requestCounts.GetOrAdd(clientIp, _ => (now, 0));
        
        // Check if we're in a new window
        if (now - windowStart > _options.Window)
        {
            windowStart = now;
            count = 0;
        }
        
        // Increment count
        count++;
        _requestCounts[clientIp] = (windowStart, count);
        
        // Add rate limit headers
        context.Response.Headers.Append("X-RateLimit-Limit", _options.RequestLimit.ToString());
        context.Response.Headers.Append("X-RateLimit-Remaining", Math.Max(0, _options.RequestLimit - count).ToString());
        context.Response.Headers.Append("X-RateLimit-Reset", windowStart.Add(_options.Window).ToString("O"));
        
        // Check if limit exceeded
        if (count > _options.RequestLimit)
        {
            _logger.LogWarning("Rate limit exceeded for IP: {ClientIp}. Count: {Count}", clientIp, count);
            
            context.Response.StatusCode = StatusCodes.Status429TooManyRequests;
            context.Response.Headers.Append("Retry-After", ((int)_options.Window.TotalSeconds).ToString());
            
            await context.Response.WriteAsJsonAsync(new
            {
                error = "Too Many Requests",
                message = $"Rate limit exceeded. Maximum {_options.RequestLimit} requests per {_options.Window.TotalMinutes} minutes.",
                retryAfter = (int)_options.Window.TotalSeconds
            });
            
            return;
        }
        
        await _next(context);
    }
    
    private string GetClientIpAddress(HttpContext context)
    {
        // Check for X-Forwarded-For header (proxy/load balancer)
        var forwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(forwardedFor))
        {
            return forwardedFor.Split(',')[0].Trim();
        }
        
        // Check for X-Real-IP header
        var realIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(realIp))
        {
            return realIp;
        }
        
        // Fallback to remote IP
        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }
    
    private void CleanupOldEntries(DateTime now)
    {
        var keysToRemove = _requestCounts
            .Where(kvp => now - kvp.Value.WindowStart > _options.Window.Add(TimeSpan.FromMinutes(5)))
            .Select(kvp => kvp.Key)
            .ToList();
        
        foreach (var key in keysToRemove)
        {
            _requestCounts.TryRemove(key, out _);
        }
    }
}

public class RateLimitOptions
{
    public int RequestLimit { get; set; } = 100;
    public TimeSpan Window { get; set; } = TimeSpan.FromMinutes(1);
}
