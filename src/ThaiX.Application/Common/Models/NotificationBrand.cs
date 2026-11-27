namespace ThaiX.Application.Common.Models;

/// <summary>
/// Shared ThaiX notification brand color tokens for consistent channel styling.
/// </summary>
public static class NotificationBrand
{
    public const string AccentHex = "#f59e0b";
    public const string PositiveHex = "#10b981";
    public const string NegativeHex = "#f43f5e";
    public const string WarningHex = "#f59e0b";
    public const string InfoHex = "#64748b";

    public const string SurfaceHex = "#11151c";
    public const string SurfaceBorderHex = "#232838";
    public const string TextPrimaryHex = "#f5f5f7";
    public const string TextMutedHex = "#c7cbd4";
    public const string TextFaintHex = "#5b6272";

    public static string SeverityColorHex(ThaiX.Domain.Aggregates.Notifications.NotificationSeverity severity) => severity switch
    {
        ThaiX.Domain.Aggregates.Notifications.NotificationSeverity.Warning => WarningHex,
        ThaiX.Domain.Aggregates.Notifications.NotificationSeverity.Error => NegativeHex,
        ThaiX.Domain.Aggregates.Notifications.NotificationSeverity.Critical => NegativeHex,
        _ => InfoHex
    };
}