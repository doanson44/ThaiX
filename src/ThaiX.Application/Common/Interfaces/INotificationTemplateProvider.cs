using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Common.Interfaces;

public interface INotificationTemplateProvider
{
    NotificationTemplate GetTemplate(NotificationKind kind, string? templateKey);
}
