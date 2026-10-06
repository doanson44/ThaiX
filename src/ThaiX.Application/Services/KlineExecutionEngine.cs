using ThaiX.Application.Trading;

namespace ThaiX.Application.Services;

public sealed class KlineExecutionEngine : IKlineExecutionEngine
{
    private readonly IKlineStrategyFactory _strategyFactory;
    private readonly IIndicatorService _indicatorService;

    public KlineExecutionEngine(IKlineStrategyFactory strategyFactory, IIndicatorService indicatorService)
    {
        _strategyFactory = strategyFactory;
        _indicatorService = indicatorService;
    }

    public KlineExecutionResult Execute(KlineExecutionContext context, IReadOnlyList<Kline> klines)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(klines);

        var strategy = _strategyFactory.Create(context);
        var indicators = _indicatorService.Compute(context, klines);

        var trend = strategy.DetectTrend(context, klines, indicators);
        var momentum = strategy.DetectMomentum(context, klines, indicators);
        var setup = strategy.DetectSetup(context, klines, indicators, trend, momentum);
        var signal = strategy.GenerateSignal(context, klines, indicators, trend, momentum, setup);

        TradePlan? trade = null;
        if (signal != SignalType.None)
        {
            trade = strategy.BuildTrade(context, klines, indicators, signal);
        }

        return new KlineExecutionResult
        {
            Trend = trend,
            Momentum = momentum,
            Setup = setup,
            Signal = signal,
            Trade = trade,
            Confidence = ComputeConfidence(trend, momentum, setup, signal, context.EventRisk)
        };
    }

    private static decimal ComputeConfidence(
        TrendDirection trend,
        MomentumStrength momentum,
        SetupType setup,
        SignalType signal,
        EventRisk eventRisk)
    {
        if (signal == SignalType.None) return 0m;

        var score = 0.35m;
        score += trend == TrendDirection.Sideways ? 0m : 0.25m;
        score += momentum == MomentumStrength.Strong ? 0.20m
               : momentum == MomentumStrength.Neutral ? 0.10m : 0m;
        score += setup == SetupType.Breakout ? 0.20m
               : setup == SetupType.Pullback ? 0.15m : 0m;

        // EventRisk reduces confidence: High = -40%, Medium = -20%
        score *= eventRisk switch
        {
            EventRisk.High => 0.60m,
            EventRisk.Medium => 0.80m,
            _ => 1.00m
        };

        return Math.Min(1.0m, score);
    }
}
