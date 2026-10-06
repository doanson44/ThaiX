namespace ThaiX.Client.Models.Notifications;

public enum NotificationKind
{
    SystemAlert = 1,
    PriceAlert = 2,
    Schedule = 3,
    Portfolio = 4,
    Market = 5,
    Blog = 6,
    Contact = 7,
    Auth = 8,
    Trading = 9,
    Lottery = 10,
    Other = 11
}

public enum NotificationChannel
{
    InApp = 1,
    Email = 2,
    Slack = 3,
    Telegram = 4,
    Sms = 5,
    Push = 6
}

public enum NotificationPreferenceSeverity
{
    Trace = 1,
    Debug = 2,
    Info = 3,
    Warning = 4,
    Error = 5
}

public enum NotificationBatchingMode
{
    Immediate = 1,
    Batched = 2,
    Digest = 3
}

public sealed record UserNotificationPreferenceDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public NotificationKind Kind { get; init; }
    public NotificationChannel Channel { get; init; }
    public bool Enabled { get; init; }
    public string? Destination { get; init; }
    public NotificationPreferenceSeverity? MinimumSeverity { get; init; }
    public TimeOnly? QuietHoursStart { get; init; }
    public TimeOnly? QuietHoursEnd { get; init; }
    public string? TimeZoneId { get; init; }
    public NotificationBatchingMode? BatchingMode { get; init; }
}

public sealed record UpsertUserNotificationPreferenceRequest
{
    public Guid UserId { get; init; }
    public NotificationKind Kind { get; init; }
    public NotificationChannel Channel { get; init; }
    public bool Enabled { get; init; }
    public string? Destination { get; init; }
    public NotificationPreferenceSeverity? MinimumSeverity { get; init; }
    public TimeOnly? QuietHoursStart { get; init; }
    public TimeOnly? QuietHoursEnd { get; init; }
    public string? TimeZoneId { get; init; }
    public NotificationBatchingMode? BatchingMode { get; init; }
}

public sealed record NotificationDetailDto
{
    public Guid Id { get; init; }
    public NotificationEventType EventType { get; init; }
    public string Text { get; init; } = string.Empty;
    public string? Title { get; init; }
    public string? Severity { get; init; }
    public string Status { get; init; } = string.Empty;
    public DateTime CreatedAtUtc { get; init; }
    public DateTime? DeliveredAtUtc { get; init; }
    public IReadOnlyList<string> Channels { get; init; } = [];
}
