using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Features.Notifications.Commands.SendNotification;

public sealed record SendNotificationCommand : IAppCommand<Guid>
{
    public NotificationKind EventType { get; init; }
    public required string Text { get; init; }
    public string? Title { get; init; }
    public NotificationSeverity? Severity { get; init; }
    public NotificationTarget Target { get; init; } = NotificationTarget.Both;
}
