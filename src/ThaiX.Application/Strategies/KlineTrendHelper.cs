using ThaiX.Application.Trading;

namespace ThaiX.Application.Strategies;

/// <summary>
/// Shared trend detection logic for all kline strategies.
/// Trend is identified by full EMA alignment: 9 > 21 > 50 (Up) or 9 &lt; 21 &lt; 50 (Down).
/// </summary>
public static class KlineTrendHelper
{
    public static TrendDirection DetectTrend(IndicatorResult indicators)
    {
        if (indicators.Ema9 > indicators.Ema21 && indicators.Ema21 > indicators.Ema50) return TrendDirection.Up;
        if (indicators.Ema9 < indicators.Ema21 && indicators.Ema21 < indicators.Ema50) return TrendDirection.Down;
        return TrendDirection.Sideways;
    }
}
