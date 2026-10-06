using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Common.Models;

/// <summary>
/// Shared "dark-finance" brand tokens (see docs/implement-plans/audit-checklist-dark-finance.md)
/// so Telegram, Slack, and Email notifications stay visually consistent instead of each
/// picking their own emoji/colors.
/// </summary>
public static class NotificationBrand
{
    public const string AccentHex = "#f59e0b";
    public const string PositiveHex = "#10b981";
    public const string NegativeHex = "#f43f5e";
    public const string InfoHex = "#5b7c99";

    public const string SurfaceHex = "#11151c";
    public const string SurfaceBorderHex = "#232838";
    public const string TextPrimaryHex = "#f5f5f7";
    public const string TextMutedHex = "#c7cbd4";
    public const string TextFaintHex = "#5b6272";

    public static string SeverityIcon(NotificationSeverity severity) => severity switch
    {
        NotificationSeverity.Debug => "⚙️",
        NotificationSeverity.Info => "🔵",
        NotificationSeverity.Warning => "🟡",
        NotificationSeverity.Error => "🔴",
        NotificationSeverity.Critical => "🆘",
        _ => "🔵"
    };

    public static string SeverityColorHex(NotificationSeverity severity) => severity switch
    {
        NotificationSeverity.Warning => AccentHex,
        NotificationSeverity.Error => NegativeHex,
        NotificationSeverity.Critical => NegativeHex,
        _ => InfoHex
    };
}
