using MediatR;

namespace ThaiX.Domain.Common.Events;

public sealed record NotificationReadyForDeliveryEvent : IDomainEvent, INotification
{
    public Guid EventId { get; }
    public DateTime OccurredAt { get; }
    public Guid NotificationId { get; }
    public DateTime? ScheduledAtUtc { get; }

    public NotificationReadyForDeliveryEvent(Guid notificationId, DateTime? scheduledAtUtc)
    {
        EventId = Guid.NewGuid();
        OccurredAt = DateTime.UtcNow;
        NotificationId = notificationId;
        ScheduledAtUtc = scheduledAtUtc;
    }
}
