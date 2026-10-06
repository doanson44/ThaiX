using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Common.Models;

public sealed record CreateNotificationRequest
{
    public required NotificationKind Kind { get; init; }

    public required NotificationSeverity Severity { get; init; }

    public required string Text { get; init; }

    public string? Title { get; init; }

    public string? TemplateKey { get; init; }

    public Guid? RecipientUserId { get; init; }

    public string? DataJson { get; init; }

    public string? SourceEventId { get; init; }

    public string? DeduplicationKey { get; init; }

    public DateTime? ScheduledAtUtc { get; init; }

    public IReadOnlyCollection<NotificationChannel> Channels { get; init; } = [];

    public NotificationCard? Card { get; init; }
}
