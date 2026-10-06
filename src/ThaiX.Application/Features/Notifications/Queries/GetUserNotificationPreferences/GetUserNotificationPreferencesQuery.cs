using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Features.Notifications.Queries.GetUserNotificationPreferences;

public sealed record GetUserNotificationPreferencesQuery(Guid UserId)
    : IAppQuery<IReadOnlyList<UserNotificationPreferenceDto>>;

public sealed record UserNotificationPreferenceDto
{
    public required Guid Id { get; init; }
    public required Guid UserId { get; init; }
    public required NotificationKind Kind { get; init; }
    public required NotificationChannel Channel { get; init; }
    public required bool Enabled { get; init; }
    public required string Destination { get; init; }
    public required NotificationSeverity MinimumSeverity { get; init; }
    public TimeOnly? QuietHoursStart { get; init; }
    public TimeOnly? QuietHoursEnd { get; init; }
    public required string TimeZoneId { get; init; }
    public required NotificationBatchingMode BatchingMode { get; init; }
}
