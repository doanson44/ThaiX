namespace ThaiX.Infrastructure.Configuration;

public sealed class PriceAlertCheckerOptions
{
    public const string SectionName = "PriceAlertChecker";

    /// <summary>
    /// IANA or Windows timezone ID used to evaluate VnStock trading hours.
    /// Defaults to Vietnam Time (UTC+7).
    /// </summary>
    public string VnStockTimeZone { get; set; } = "SE Asia Standard Time";

    /// <summary>
    /// Start of VnStock trading window, local time (HH:mm). Inclusive.
    /// </summary>
    public string VnStockTradingStart { get; set; } = "09:30";

    /// <summary>
    /// End of VnStock trading window, local time (HH:mm). Inclusive.
    /// </summary>
    public string VnStockTradingEnd { get; set; } = "15:00";
}
