using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Common.Interfaces;

public interface INotificationChannelProvider
{
    NotificationChannel Channel { get; }

    string Provider { get; }

    Task<NotificationSendResult> SendAsync(
        NotificationRenderedMessage message,
        CancellationToken cancellationToken);
}
