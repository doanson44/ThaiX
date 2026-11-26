using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Application.Common.Models;

/// <summary>
/// Shared ThaiX notification brand tokens for consistent Slack, Telegram, and email output.
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

    public const string BrandName = "ThaiX";
    public const string BrandDescriptor = "FINANCIAL INTELLIGENCE";

    public static string SeverityLabel(NotificationSeverity severity) => severity switch
    {
        NotificationSeverity.Debug => "DEBUG",
        NotificationSeverity.Info => "INFO",
        NotificationSeverity.Warning => "WARNING",
        NotificationSeverity.Error => "ERROR",
        NotificationSeverity.Critical => "CRITICAL",
        _ => "INFO"
    };

    public static string KindLabel(NotificationKind kind) => kind switch
    {
        NotificationKind.MarketOverview => "MARKET OVERVIEW",
        NotificationKind.PriceUpdate => "PRICE UPDATE",
        NotificationKind.CryptoSpike => "MARKET MOVEMENT",
        NotificationKind.GoodEntryCrypto => "CRYPTO RESEARCH",
        NotificationKind.GoodEntryStocks => "EQUITY RESEARCH",
        NotificationKind.SystemAlert => "SYSTEM ALERT",
        NotificationKind.DataSync => "DATA OPERATIONS",
        NotificationKind.MarketScanner => "MARKET SCANNER",
        NotificationKind.PriceAlert => "PRICE ALERT",
        NotificationKind.PortfolioUpdate => "PORTFOLIO",
        NotificationKind.TradeSignal => "TRADE SIGNAL",
        _ => "NOTIFICATION"
    };

    public static string CardTypeLabel(NotificationCardType type) => type switch
    {
        NotificationCardType.Metric => "MARKET DATA",
        NotificationCardType.Signal => "SIGNAL METRICS",
        NotificationCardType.Alert => "ALERT DETAILS",
        NotificationCardType.Summary => "KEY FIGURES",
        NotificationCardType.Analysis => "ANALYSIS",
        NotificationCardType.Error => "INCIDENT DETAILS",
        _ => "DETAILS"
    };

    public static string SeverityIcon(NotificationSeverity severity) => severity switch
    {
        NotificationSeverity.Debug => "[DEBUG]",
        NotificationSeverity.Info => "[INFO]",
        NotificationSeverity.Warning => "[WARNING]",
        NotificationSeverity.Error => "[ERROR]",
        NotificationSeverity.Critical => "[CRITICAL]",
        _ => "[INFO]"
    };

    public static string SeverityColorHex(NotificationSeverity severity) => severity switch
    {
        NotificationSeverity.Warning => WarningHex,
        NotificationSeverity.Error => NegativeHex,
        NotificationSeverity.Critical => NegativeHex,
        _ => InfoHex
    };
}
