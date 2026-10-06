using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Common.Models;

public sealed record NotificationRouteRequest
{
    public required NotificationKind Kind { get; init; }

    public required NotificationSeverity Severity { get; init; }

    public Guid? RecipientUserId { get; init; }

    public IReadOnlyCollection<NotificationChannel> Channels { get; init; } = [];
}
