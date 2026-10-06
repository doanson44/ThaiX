using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Common.Interfaces;

public interface INotificationFormatter
{
    NotificationChannel Channel { get; }

    string Format(NotificationCard card);
}
