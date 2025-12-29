using Fidelity.Domain.Common;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Fidelity.Application.Common.Behaviors;

/// <summary>
/// Dispatches domain events using MediatR - ISO 25000: Modularity
/// </summary>
public class DomainEventDispatcher
{
    private readonly IPublisher _publisher;
    private readonly ILogger<DomainEventDispatcher> _logger;

    public DomainEventDispatcher(IPublisher publisher, ILogger<DomainEventDispatcher> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public async Task DispatchEventsAsync(IEnumerable<BaseEntity> entities, CancellationToken cancellationToken = default)
    {
        var entitiesList = entities.ToList();
        
        foreach (var entity in entitiesList)
        {
            var events = entity.DomainEvents.ToList();
            entity.ClearDomainEvents();

            foreach (var domainEvent in events)
            {
                _logger.LogDebug("Dispatching domain event: {EventType}", domainEvent.EventType);
                await _publisher.Publish(domainEvent, cancellationToken);
            }
        }
    }
}
