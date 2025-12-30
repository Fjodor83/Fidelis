using Fidelity.Application.Common.Interfaces;
using Fidelity.Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Security.Cryptography;
using System.Text;

namespace Fidelity.Application.Common.Behaviors;

public class IdempotencyBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IIdempotentCommand
{
    private readonly IApplicationDbContext _context;
    
    public IdempotencyBehavior(IApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken ct)
    {
        var requestHash = ComputeHash(request);
        
        // Check if already processed
        var existingResult = await _context.IdempotencyRecords
            .FirstOrDefaultAsync(r => r.RequestHash == requestHash, ct);
            
        if (existingResult != null)
        {
            return JsonSerializer.Deserialize<TResponse>(existingResult.Result)!;
        }
        
        // Process
        var response = await next();
        
        // Store result
        _context.IdempotencyRecords.Add(new IdempotencyRecord
        {
            RequestHash = requestHash,
            Result = JsonSerializer.Serialize(response),
            CreatedAt = DateTime.UtcNow
        });
        
        await _context.SaveChangesAsync(ct);
        
        return response;
    }

    private string ComputeHash(TRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        using var sha256 = SHA256.Create();
        var bytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(json));
        return Convert.ToBase64String(bytes);
    }
}
