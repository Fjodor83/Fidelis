using MediatR;

namespace Fidelity.Domain.Common;

/// <summary>
/// Marker interface for domain events - enables CQRS pattern
/// </summary>
public interface IDomainEvent : INotification
{
    DateTime OccurredOn { get; }
    string EventType { get; }
}

/// <summary>
/// Base class for domain events with common properties
/// </summary>
public abstract class DomainEventBase : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public string EventType => GetType().Name;
    public Guid EventId { get; } = Guid.NewGuid();
}
