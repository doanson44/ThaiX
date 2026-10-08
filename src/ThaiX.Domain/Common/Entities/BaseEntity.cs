using ThaiX.Domain.Common.Events;

namespace ThaiX.Domain.Common.Entities;

/// <summary>
/// Base class for all domain entities.
/// Provides identity, timestamps, concurrency control, and domain events.
/// </summary>
public abstract class BaseEntity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    /// <summary>
    /// Unique identifier for this entity.
    /// </summary>
    public Guid Id { get; protected set; }

    /// <summary>
    /// When this entity was created.
    /// Set automatically by infrastructure interceptor.
    /// </summary>
    public DateTime CreatedAt { get; protected set; }

    /// <summary>
    /// When this entity was last modified.
    /// Set automatically by infrastructure interceptor.
    /// </summary>
    public DateTime? UpdatedAt { get; protected set; }

    /// <summary>
    /// Optimistic concurrency control token.
    /// Backed by a MySQL TIMESTAMP(6) column with ON UPDATE CURRENT_TIMESTAMP(6);
    /// the database engine generates the value, EF Core never writes it.
    /// </summary>
    public DateTime RowVersion { get; protected set; }

    /// <summary>
    /// Domain events raised by this entity.
    /// </summary>
    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    /// <summary>
    /// Adds a domain event to be dispatched after persistence.
    /// </summary>
    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    /// <summary>
    /// Clears all domain events.
    /// Called by infrastructure after events are persisted to Outbox.
    /// </summary>
    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }

    /// <summary>
    /// Updates the UpdatedAt timestamp.
    /// Called by infrastructure interceptor, not domain logic.
    /// </summary>
    internal void SetUpdatedAt(DateTime updatedAt)
    {
        UpdatedAt = updatedAt;
    }
}
