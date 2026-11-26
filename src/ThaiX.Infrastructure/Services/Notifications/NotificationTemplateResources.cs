using System.Globalization;
using System.Resources;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Services.Notifications;

internal static class NotificationTemplateResources
{
    private static readonly ResourceManager ResourceManager = new(
        "ThaiX.Infrastructure.Services.Notifications.Resources.NotificationTemplates",
        typeof(NotificationTemplateResources).Assembly);

    public static string Get(string key) =>
        ResourceManager.GetString(key, CultureInfo.CurrentUICulture) ?? key;

    public static string SeverityLabel(NotificationSeverity severity) =>
        Get($"SeverityLabel.{severity}");

    public static string SeverityIcon(NotificationSeverity severity) =>
        Get($"SeverityIcon.{severity}");

    public static string KindLabel(NotificationKind kind) =>
        Get($"KindLabel.{kind}");

    public static string CardTypeLabel(NotificationCardType type) =>
        Get($"CardTypeLabel.{type}");

    public static string Subject(NotificationKind kind) =>
        Get($"Subject.{kind}");
}
