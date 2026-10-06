namespace ThaiX.Domain.Aggregates.Notifications;

public enum NotificationDeliveryStatus
{
    Pending = 1,
    Rendering = 2,
    Sending = 3,
    Sent = 4,
    RetryScheduled = 5,
    Failed = 6,
    Skipped = 7,
    Cancelled = 8
}
