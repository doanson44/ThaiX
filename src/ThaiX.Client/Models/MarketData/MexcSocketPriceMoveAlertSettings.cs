namespace ThaiX.Client.Models.MarketData;

public sealed class MexcSocketPriceMoveAlertSettings
{
    public int TimeWindowMinutes { get; set; } = 3;

    /// <summary>
    /// Absolute percent threshold (e.g. 10 means ±10% vs the oldest price in the window).
    /// </summary>
    public decimal AbsoluteChangePercent { get; set; } = 10m;

    public MexcSocketPriceMoveAlertSettings Clone() => new()
    {
        TimeWindowMinutes = TimeWindowMinutes,
        AbsoluteChangePercent = AbsoluteChangePercent
    };
}

/// <summary>Browser-persisted switch + settings for client-side MEXC socket alerts.</summary>
public sealed class MexcSocketPriceMoveAlertState
{
    public bool Enabled { get; set; }
    public int TimeWindowMinutes { get; set; } = 3;
    public decimal AbsoluteChangePercent { get; set; } = 10m;

    public MexcSocketPriceMoveAlertSettings ToSettings() => new()
    {
        TimeWindowMinutes = TimeWindowMinutes < 1 ? 3 : TimeWindowMinutes,
        AbsoluteChangePercent = AbsoluteChangePercent <= 0m ? 10m : AbsoluteChangePercent
    };

    public static MexcSocketPriceMoveAlertState From(bool enabled, MexcSocketPriceMoveAlertSettings settings) => new()
    {
        Enabled = enabled,
        TimeWindowMinutes = settings.TimeWindowMinutes,
        AbsoluteChangePercent = settings.AbsoluteChangePercent
    };
}

public sealed record MexcSocketPriceMoveAlert(
    string Symbol,
    decimal ReferencePrice,
    decimal CurrentPrice,
    decimal ChangePercent,
    int TimeWindowMinutes);
