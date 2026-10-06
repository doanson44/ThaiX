using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.Notifications;

public sealed class UserNotificationPreference : BaseAuditableEntity
{
    private UserNotificationPreference()
    {
    }

    public Guid UserId { get; private set; }

    public NotificationKind Kind { get; private set; }

    public NotificationChannel Channel { get; private set; }

    public bool Enabled { get; private set; }

    public string Destination { get; private set; } = null!;

    public NotificationSeverity MinimumSeverity { get; private set; }

    public TimeOnly? QuietHoursStart { get; private set; }

    public TimeOnly? QuietHoursEnd { get; private set; }

    public string TimeZoneId { get; private set; } = null!;

    public NotificationBatchingMode BatchingMode { get; private set; }

    public static UserNotificationPreference Create(
        Guid userId,
        NotificationKind kind,
        NotificationChannel channel,
        string destination,
        NotificationSeverity minimumSeverity,
        string timeZoneId,
        NotificationBatchingMode batchingMode = NotificationBatchingMode.Immediate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);

        return new UserNotificationPreference
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Kind = kind,
            Channel = channel,
            Enabled = true,
            Destination = destination.Trim(),
            MinimumSeverity = minimumSeverity,
            TimeZoneId = timeZoneId.Trim(),
            BatchingMode = batchingMode
        };
    }

    public bool Allows(NotificationSeverity severity)
    {
        return Enabled
            && BatchingMode != NotificationBatchingMode.Disabled
            && severity >= MinimumSeverity;
    }

    public void Update(
        bool enabled,
        string destination,
        NotificationSeverity minimumSeverity,
        TimeOnly? quietHoursStart,
        TimeOnly? quietHoursEnd,
        string timeZoneId,
        NotificationBatchingMode batchingMode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(destination);
        ArgumentException.ThrowIfNullOrWhiteSpace(timeZoneId);

        Enabled = enabled;
        Destination = destination.Trim();
        MinimumSeverity = minimumSeverity;
        QuietHoursStart = quietHoursStart;
        QuietHoursEnd = quietHoursEnd;
        TimeZoneId = timeZoneId.Trim();
        BatchingMode = batchingMode;
    }

    public void Disable()
    {
        Enabled = false;
    }

    public void Enable()
    {
        Enabled = true;
    }

    public void SoftDelete()
    {
        Delete();
    }
}
