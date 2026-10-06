using ThaiX.Application.Trading;

namespace ThaiX.Application.Strategies;

public sealed class CryptoStrategy : IKlineStrategy
{
    // RSI thresholds for crypto market (higher volatility — stronger signal required)
    private const decimal RsiStrongThreshold = 60m;
    private const decimal RsiNeutralThreshold = 48m;

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

        // Breakout checked first — higher priority in crypto due to momentum-driven moves
        if (close > indicators.HighestHigh20) return SetupType.Breakout;

        var nearEma9 = Math.Abs(close - indicators.Ema9) <= indicators.Atr14 * 0.8m;
        var nearEma21 = Math.Abs(close - indicators.Ema21) <= indicators.Atr14 * 0.8m;
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
        if (context.MarketRegime == MarketRegime.RiskOff) return SignalType.None;
        if (setup == SetupType.None || momentum != MomentumStrength.Strong) return SignalType.None;

        if (trend == TrendDirection.Up) return SignalType.Long;
        if (trend == TrendDirection.Down) return SignalType.Short;

        return SignalType.None;
    }

    public TradePlan BuildTrade(KlineExecutionContext context, IReadOnlyList<Kline> klines, IndicatorResult indicators, SignalType signal)
    {
        if (signal == SignalType.None)
        {
            throw new ArgumentException("Cannot build trade for NONE signal.");
        }

        var entry = klines[^1].Close;
        var stopDistance = indicators.Atr14 * 1.5m;
        var swingRange = indicators.HighestHigh20 - indicators.LowestLow20;

        if (signal == SignalType.Long)
        {
            return new TradePlan
            {
                Side = SignalType.Long,
                Entry = entry,
                StopLoss = entry - stopDistance,
                TakeProfit1 = entry + swingRange * Fibo618,
                TakeProfit2 = entry + swingRange * Fibo1000
            };
        }

        return new TradePlan
        {
            Side = SignalType.Short,
            Entry = entry,
            StopLoss = entry + stopDistance,
            TakeProfit1 = entry - swingRange * Fibo618,
            TakeProfit2 = entry - swingRange * Fibo1000
        };
    }
}
