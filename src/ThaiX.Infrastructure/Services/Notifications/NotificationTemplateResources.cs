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
        Enum.IsDefined(kind) ? Get($"KindLabel.{kind}") : Get("KindLabel.Default");

    public static string CardTypeLabel(NotificationCardType type) =>
        Enum.IsDefined(type) ? Get($"CardTypeLabel.{type}") : Get("CardTypeLabel.Default");

    public static string Subject(NotificationKind kind) =>
        Enum.IsDefined(kind) ? Get($"Subject.{kind}") : Get("Subject.Default");
}
