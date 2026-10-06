using MediatR;

namespace ThaiX.Domain.Common.Events;

/// <summary>
/// Raised when a contact's profile (full name or avatar) is updated.
/// Used to sync the UserProfile read model without coupling Contact to Identity.
/// Implements INotification so MediatR can dispatch to INotificationHandler.
/// </summary>
public sealed record ContactProfileUpdatedEvent : IDomainEvent, INotification
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public Guid ContactId { get; }
    public string FullName { get; }
    public string? AvatarUrl { get; }

    public ContactProfileUpdatedEvent(Guid contactId, string fullName, string? avatarUrl)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        ContactId = contactId;
        FullName = fullName ?? string.Empty;
        AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
    }
}
