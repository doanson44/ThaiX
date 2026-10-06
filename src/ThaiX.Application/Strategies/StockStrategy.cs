using ThaiX.Application.Trading;

namespace ThaiX.Application.Strategies;

public sealed class StockStrategy : IKlineStrategy
{
    // RSI thresholds for stock market (less volatile than crypto)
    private const decimal RsiStrongThreshold = 55m;
    private const decimal RsiNeutralThreshold = 45m;

    // Fibonacci extension levels for TP projection from swing range
    private const decimal Fibo618 = 0.618m;
    private const decimal Fibo1000 = 1.000m;

    public TrendDirection DetectTrend(KlineExecutionContext context, IReadOnlyList<Kline> klines, IndicatorResult indicators)
        => KlineTrendHelper.DetectTrend(indicators);

    public MomentumStrength DetectMomentum(KlineExecutionContext context, IReadOnlyList<Kline> klines, IndicatorResult indicators)
    {
        if (indicators.Rsi14 > RsiStrongThreshold) return MomentumStrength.Strong;
        if (indicators.Rsi14 > RsiNeutralThreshold) return MomentumStrength.Neutral;
        return MomentumStrength.Weak;
    }

    public SetupType DetectSetup(
        KlineExecutionContext context,
        IReadOnlyList<Kline> klines,
        IndicatorResult indicators,
        TrendDirection trend,
        MomentumStrength momentum)
    {
        var close = klines[^1].Close;

        // Breakout checked first — stronger signal than a pullback
        if (close > indicators.HighestHigh20) return SetupType.Breakout;

        var nearEma9 = Math.Abs(close - indicators.Ema9) <= indicators.Atr14 * 0.5m;
        var nearEma21 = Math.Abs(close - indicators.Ema21) <= indicators.Atr14 * 0.5m;
        if (nearEma9 || nearEma21) return SetupType.Pullback;

        return SetupType.None;
    }

    public SignalType GenerateSignal(
        KlineExecutionContext context,
        IReadOnlyList<Kline> klines,
        IndicatorResult indicators,
        TrendDirection trend,
        MomentumStrength momentum,
        SetupType setup)
    {
        if (context.EventRisk == EventRisk.High) return SignalType.None;
        if (context.MarketRegime != MarketRegime.RiskOn) return SignalType.None;

        if (trend == TrendDirection.Up && momentum == MomentumStrength.Strong && setup != SetupType.None)
        {
            return SignalType.Long;
        }

        return SignalType.None;
    }

    public TradePlan BuildTrade(KlineExecutionContext context, IReadOnlyList<Kline> klines, IndicatorResult indicators, SignalType signal)
    {
        if (signal != SignalType.Long)
        {
            throw new ArgumentException("Stock strategy only supports LONG trades.");
        }

        var entry = klines[^1].Close;
        var stop = entry - indicators.Atr14 * 2m;
        var swingRange = indicators.HighestHigh20 - indicators.LowestLow20;

        return new TradePlan
        {
            Side = SignalType.Long,
            Entry = entry,
            StopLoss = stop,
            TakeProfit1 = entry + swingRange * Fibo618,
            TakeProfit2 = entry + swingRange * Fibo1000
        };
    }
}
