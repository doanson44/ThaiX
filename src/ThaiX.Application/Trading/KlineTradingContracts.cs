namespace ThaiX.Application.Trading;

public enum MarketType
{
    Stock = 1,
    Crypto = 2,
    CryptoSpot = 3
}

public enum MarketRegime
{
    Neutral = 0,
    RiskOn = 1,
    RiskOff = 2
}

public enum EventRisk
{
    Low = 0,
    Medium = 1,
    High = 2
}

public enum TrendDirection
{
    Sideways = 0,
    Up = 1,
    Down = 2
}

public enum MomentumStrength
{
    Weak = 0,
    Neutral = 1,
    Strong = 2
}

public enum SetupType
{
    None = 0,
    Pullback = 1,
    Breakout = 2
}

public enum SignalType
{
    None = 0,
    Long = 1,
    Short = 2
}

public sealed record Kline
{
    public required DateTime Time { get; init; }
    public required decimal Open { get; init; }
    public required decimal High { get; init; }
    public required decimal Low { get; init; }
    public required decimal Close { get; init; }
    public required decimal Volume { get; init; }
}

public sealed record IndicatorResult
{
    public required decimal Ema9 { get; init; }
    public required decimal Ema21 { get; init; }
    public required decimal Ema50 { get; init; }
    public required decimal Rsi14 { get; init; }
    public required decimal Atr14 { get; init; }
    public required decimal AverageVolume20 { get; init; }
    public required decimal LatestClose { get; init; }
    public required decimal HighestHigh20 { get; init; }
    public required decimal LowestLow20 { get; init; }
}

public sealed record KlineExecutionContext
{
    public required MarketType MarketType { get; init; }
    public required string Timeframe { get; init; }
    public MarketRegime MarketRegime { get; init; } = MarketRegime.Neutral;
    public EventRisk EventRisk { get; init; } = EventRisk.Low;
}

public sealed record TradePlan
{
    public required SignalType Side { get; init; }
    public required decimal Entry { get; init; }
    public required decimal StopLoss { get; init; }
    public required decimal TakeProfit1 { get; init; }
    public required decimal TakeProfit2 { get; init; }
}

public sealed record KlineExecutionResult
{
    public required TrendDirection Trend { get; init; }
    public required MomentumStrength Momentum { get; init; }
    public required SetupType Setup { get; init; }
    public required SignalType Signal { get; init; }
    public TradePlan? Trade { get; init; }
    public required decimal Confidence { get; init; }
}

public interface IIndicatorService
{
    int GetMinimumBars(KlineExecutionContext context);
    IndicatorResult Compute(KlineExecutionContext context, IReadOnlyList<Kline> klines);
}

public interface IKlineExecutionEngine
{
    KlineExecutionResult Execute(KlineExecutionContext context, IReadOnlyList<Kline> klines);
}

public interface IKlineStrategy
{
    TrendDirection DetectTrend(KlineExecutionContext context, IReadOnlyList<Kline> klines, IndicatorResult indicators);

    MomentumStrength DetectMomentum(KlineExecutionContext context, IReadOnlyList<Kline> klines, IndicatorResult indicators);

    SetupType DetectSetup(
        KlineExecutionContext context,
        IReadOnlyList<Kline> klines,
        IndicatorResult indicators,
        TrendDirection trend,
        MomentumStrength momentum);

    SignalType GenerateSignal(
        KlineExecutionContext context,
        IReadOnlyList<Kline> klines,
        IndicatorResult indicators,
        TrendDirection trend,
        MomentumStrength momentum,
        SetupType setup);

    TradePlan BuildTrade(KlineExecutionContext context, IReadOnlyList<Kline> klines, IndicatorResult indicators, SignalType signal);
}

public interface IKlineStrategyFactory
{
    IKlineStrategy Create(KlineExecutionContext context);
}
