using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Common.Models;

public sealed record NotificationRenderedMessage
{
    public required NotificationChannel Channel { get; init; }

    public required string Provider { get; init; }

    public required string Destination { get; init; }

    public required string Text { get; init; }

    public required string PayloadJson { get; init; }

    public required string IdempotencyKey { get; init; }

    /// <summary>Slack Block Kit attachments JSON (color bar + blocks), when the channel supports it.</summary>
    public string? AttachmentsJson { get; init; }
}
