using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Common.Models;

public sealed record NotificationRoute
{
    public required NotificationChannel Channel { get; init; }

    public required string Provider { get; init; }

    public required string Destination { get; init; }
}
