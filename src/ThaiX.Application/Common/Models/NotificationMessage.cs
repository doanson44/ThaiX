using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Common.Models;

public sealed class NotificationMessage
{
    public NotificationKind Kind { get; init; }

    public NotificationSeverity Severity { get; init; } = NotificationSeverity.Info;

    public string Text { get; init; } = string.Empty;

    public string? Title { get; init; }

    public string? TemplateKey { get; init; }

    public Guid? RecipientUserId { get; init; }

    public string? DataJson { get; init; }

    public string? SourceEventId { get; init; }

    public string? DeduplicationKey { get; init; }

    public DateTime? ScheduledAtUtc { get; init; }

    public IReadOnlyCollection<NotificationChannel> Channels { get; init; } = [];

    public IReadOnlyCollection<NotificationSection> Sections { get; init; } = [];

    public IReadOnlyCollection<NotificationAction> Actions { get; init; } = [];

    public NotificationCard? Card { get; init; }
}
