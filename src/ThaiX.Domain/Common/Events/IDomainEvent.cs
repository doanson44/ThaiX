namespace ThaiX.Domain.Common.Events;

/// <summary>
/// Marker interface for domain events.
/// Domain events represent something that happened in the domain.
/// They are dispatched asynchronously via the Outbox pattern.
/// </summary>
public interface IDomainEvent
{
    /// <summary>
    /// When this event occurred.
    /// </summary>
    DateTime OccurredAt { get; }

    /// <summary>
    /// Unique identifier for this event instance.
    /// </summary>
    Guid EventId { get; }
}
