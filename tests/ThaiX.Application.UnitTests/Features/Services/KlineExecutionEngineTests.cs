using ThaiX.Application.Services;
using ThaiX.Application.Trading;

namespace ThaiX.Application.UnitTests.Features.Services;

public sealed class KlineExecutionEngineTests
{
    private static readonly IReadOnlyList<Kline> Klines = new[]
    {
        new Kline { Time = DateTime.UtcNow.AddMinutes(-10), Open = 100m, High = 105m, Low = 98m, Close = 102m, Volume = 1m },
        new Kline { Time = DateTime.UtcNow.AddMinutes(-5), Open = 102m, High = 106m, Low = 101m, Close = 105m, Volume = 1m }
    };

    [Fact]
    public void Execute_WhenSignalNone_ShouldReturnNoTradeAndZeroConfidence()
    {
        var context = new KlineExecutionContext { MarketType = MarketType.CryptoSpot, Timeframe = "1h", MarketRegime = MarketRegime.RiskOn };
        var indicator = new IndicatorResult
        {
            Ema9 = 1m,
            Ema21 = 1m,
            Ema50 = 1m,
            Rsi14 = 10m,
            Atr14 = 1m,
            AverageVolume20 = 0m,
            LatestClose = 105m,
            HighestHigh20 = 110m,
            LowestLow20 = 95m
        };

        var strategyMock = new Mock<IKlineStrategy>();
        strategyMock.Setup(x => x.DetectTrend(context, Klines, indicator)).Returns(TrendDirection.Sideways);
        strategyMock.Setup(x => x.DetectMomentum(context, Klines, indicator)).Returns(MomentumStrength.Weak);
        strategyMock.Setup(x => x.DetectSetup(context, Klines, indicator, TrendDirection.Sideways, MomentumStrength.Weak)).Returns(SetupType.None);
        strategyMock.Setup(x => x.GenerateSignal(context, Klines, indicator, TrendDirection.Sideways, MomentumStrength.Weak, SetupType.None)).Returns(SignalType.None);

        var factoryMock = new Mock<IKlineStrategyFactory>();
        factoryMock.Setup(x => x.Create(context)).Returns(strategyMock.Object);

        var indicatorMock = new Mock<IIndicatorService>();
        indicatorMock.Setup(x => x.Compute(context, Klines)).Returns(indicator);

        var engine = new KlineExecutionEngine(factoryMock.Object, indicatorMock.Object);
        var result = engine.Execute(context, Klines);

        result.Signal.Should().Be(SignalType.None);
        result.Trade.Should().BeNull();
        result.Confidence.Should().Be(0m);
    }

    [Fact]
    public void Execute_WhenSignalLong_ShouldReturnTradeAndPositiveConfidence()
    {
        var context = new KlineExecutionContext { MarketType = MarketType.CryptoSpot, Timeframe = "1h", MarketRegime = MarketRegime.RiskOn };
        var indicator = new IndicatorResult
        {
            Ema9 = 105m,
            Ema21 = 100m,
            Ema50 = 95m,
            Rsi14 = 70m,
            Atr14 = 2m,
            AverageVolume20 = 0m,
            LatestClose = 105m,
            HighestHigh20 = 110m,
            LowestLow20 = 95m
        };

        var strategyMock = new Mock<IKlineStrategy>();
        strategyMock.Setup(x => x.DetectTrend(context, Klines, indicator)).Returns(TrendDirection.Up);
        strategyMock.Setup(x => x.DetectMomentum(context, Klines, indicator)).Returns(MomentumStrength.Strong);
        strategyMock.Setup(x => x.DetectSetup(context, Klines, indicator, TrendDirection.Up, MomentumStrength.Strong)).Returns(SetupType.Pullback);
        strategyMock.Setup(x => x.GenerateSignal(context, Klines, indicator, TrendDirection.Up, MomentumStrength.Strong, SetupType.Pullback)).Returns(SignalType.Long);
        strategyMock.Setup(x => x.BuildTrade(context, Klines, indicator, SignalType.Long)).Returns(new TradePlan
        {
            Side = SignalType.Long,
            Entry = 105m,
            StopLoss = 100m,
            TakeProfit1 = 110m,
            TakeProfit2 = 115m
        });

        var factoryMock = new Mock<IKlineStrategyFactory>();
        factoryMock.Setup(x => x.Create(context)).Returns(strategyMock.Object);

        var indicatorMock = new Mock<IIndicatorService>();
        indicatorMock.Setup(x => x.Compute(context, Klines)).Returns(indicator);

        var engine = new KlineExecutionEngine(factoryMock.Object, indicatorMock.Object);
        var result = engine.Execute(context, Klines);

        result.Signal.Should().Be(SignalType.Long);
        result.Trade.Should().NotBeNull();
        result.Confidence.Should().BeGreaterThan(0m);
    }
}
