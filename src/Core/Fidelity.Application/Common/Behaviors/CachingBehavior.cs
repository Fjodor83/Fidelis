using Fidelity.Application.Common.Interfaces;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Fidelity.Application.Common.Behaviors;

public class CachingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : ICacheableQuery
{
    private readonly IDistributedCache _cache;
    
    public CachingBehavior(IDistributedCache cache)
    {
        _cache = cache;
    }
    
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken ct)
    {
        var cacheKey = request.CacheKey;
        
        // Try get from cache
        var cachedResponse = await _cache.GetStringAsync(cacheKey, ct);
        if (cachedResponse != null)
        {
            return JsonSerializer.Deserialize<TResponse>(cachedResponse)!;
        }
        
        // Execute query
        var response = await next();
        
        // Store in cache
        var serialized = JsonSerializer.Serialize(response);
        await _cache.SetStringAsync(
            cacheKey, 
            serialized, 
            new DistributedCacheEntryOptions 
            { 
                AbsoluteExpirationRelativeToNow = request.CacheDuration 
            },
            ct
        );
        
        return response;
    }
}
