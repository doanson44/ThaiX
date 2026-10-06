namespace ThaiX.Domain.Aggregates.Notifications;

public enum NotificationStatus
{
    Pending = 1,
    Ready = 2,
    PartiallyDelivered = 3,
    Delivered = 4,
    Cancelled = 5,
    Failed = 6
}
