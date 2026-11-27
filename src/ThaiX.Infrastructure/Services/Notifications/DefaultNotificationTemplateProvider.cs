using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Services.Notifications;

public sealed class DefaultNotificationTemplateProvider : INotificationTemplateProvider
{
    public NotificationTemplate GetTemplate(NotificationKind kind, string? templateKey)
    {
        var key = string.IsNullOrWhiteSpace(templateKey)
            ? $"{kind}.Default"
            : templateKey.Trim();

        return new NotificationTemplate
        {
            Key = key,
            Kind = kind,
            DefaultSubject = NotificationTemplateResources.Subject(kind)
        };
    }
}