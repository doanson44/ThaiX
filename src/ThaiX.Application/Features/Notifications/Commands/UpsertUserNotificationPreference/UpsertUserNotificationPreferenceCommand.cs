using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Features.Notifications.Commands.UpsertUserNotificationPreference;

public sealed record UpsertUserNotificationPreferenceCommand : IAppCommand<Guid>
{
    public required Guid UserId { get; init; }
    public required NotificationKind Kind { get; init; }
    public required NotificationChannel Channel { get; init; }
    public required bool Enabled { get; init; }
    public required string Destination { get; init; }
    public NotificationSeverity MinimumSeverity { get; init; } = NotificationSeverity.Info;
    public TimeOnly? QuietHoursStart { get; init; }
    public TimeOnly? QuietHoursEnd { get; init; }
    public string TimeZoneId { get; init; } = "UTC";
    public NotificationBatchingMode BatchingMode { get; init; } = NotificationBatchingMode.Immediate;
}
