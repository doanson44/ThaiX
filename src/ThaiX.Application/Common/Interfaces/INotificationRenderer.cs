using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Common.Interfaces;

public interface INotificationRenderer
{
    NotificationChannel Channel { get; }

    NotificationRenderedMessage Render(NotificationMessage message, NotificationRoute route, string idempotencyKey);
}
