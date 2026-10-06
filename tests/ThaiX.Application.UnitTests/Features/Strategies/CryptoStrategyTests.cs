using ThaiX.Application.Strategies;
using ThaiX.Application.Trading;

namespace ThaiX.Application.UnitTests.Features.Strategies;

public sealed class CryptoStrategyTests
{
    private static readonly IReadOnlyList<Kline> Klines = new[]
    {
        new Kline { Time = DateTime.UtcNow.AddMinutes(-30), Open = 95m, High = 100m, Low = 90m, Close = 95m, Volume = 1m },
        new Kline { Time = DateTime.UtcNow.AddMinutes(-20), Open = 95m, High = 105m, Low = 94m, Close = 100m, Volume = 1m },
        new Kline { Time = DateTime.UtcNow.AddMinutes(-10), Open = 100m, High = 110m, Low = 98m, Close = 108m, Volume = 1m }
    };

    private static IndicatorResult CreateIndicator(decimal rsi14, decimal highestHigh20, decimal lowestLow20, decimal ema9, decimal ema21, decimal atr14)
        => new()
        {
            Ema9 = ema9,
            Ema21 = ema21,
            Ema50 = 0m,
            Rsi14 = rsi14,
            Atr14 = atr14,
            AverageVolume20 = 0m,
            LatestClose = 108m,
            HighestHigh20 = highestHigh20,
            LowestLow20 = lowestLow20
        };

    [Theory]
    [InlineData(61, MomentumStrength.Strong)]
    [InlineData(50, MomentumStrength.Neutral)]
    [InlineData(30, MomentumStrength.Weak)]
    public void DetectMomentum_ShouldReturnExpectedStrength(decimal rsi, MomentumStrength expected)
    {
        var strategy = new CryptoStrategy();
        var result = strategy.DetectMomentum(new KlineExecutionContext { MarketType = MarketType.CryptoSpot, Timeframe = "1h" }, Klines, CreateIndicator(rsi, 0m, 0m, 0m, 0m, 1m));

        result.Should().Be(expected);
    }

    [Fact]
    public void DetectSetup_WhenCloseAboveHighestHigh_ShouldReturnBreakout()
    {
        var strategy = new CryptoStrategy();
        var result = strategy.DetectSetup(
            new KlineExecutionContext { MarketType = MarketType.CryptoSpot, Timeframe = "1h" },
            Klines,
            CreateIndicator(70m, 105m, 90m, 100m, 95m, 2m),
            TrendDirection.Up,
            MomentumStrength.Strong);

        result.Should().Be(SetupType.Breakout);
    }

    [Fact]
    public void DetectSetup_WhenCloseNearEma_ShouldReturnPullback()
    {
        var strategy = new CryptoStrategy();
        var result = strategy.DetectSetup(
            new KlineExecutionContext { MarketType = MarketType.CryptoSpot, Timeframe = "1h" },
            Klines,
            CreateIndicator(70m, 200m, 90m, 108m, 110m, 0.5m),
            TrendDirection.Up,
            MomentumStrength.Strong);

        result.Should().Be(SetupType.Pullback);
    }

    [Fact]
    public void GenerateSignal_WhenRiskOff_ShouldReturnNone()
    {
        var strategy = new CryptoStrategy();
        var signal = strategy.GenerateSignal(
            new KlineExecutionContext { MarketType = MarketType.CryptoSpot, Timeframe = "1h", MarketRegime = MarketRegime.RiskOff },
            Klines,
            CreateIndicator(70m, 110m, 90m, 100m, 95m, 1m),
            TrendDirection.Up,
            MomentumStrength.Strong,
            SetupType.Pullback);

        signal.Should().Be(SignalType.None);
    }

    [Fact]
    public void GenerateSignal_WithStrongMomentumUpTrendPullback_ShouldReturnLong()
    {
        var strategy = new CryptoStrategy();
        var signal = strategy.GenerateSignal(
            new KlineExecutionContext { MarketType = MarketType.CryptoSpot, Timeframe = "1h", MarketRegime = MarketRegime.RiskOn },
            Klines,
            CreateIndicator(70m, 110m, 90m, 100m, 95m, 1m),
            TrendDirection.Up,
            MomentumStrength.Strong,
            SetupType.Pullback);

        signal.Should().Be(SignalType.Long);
    }

    [Fact]
    public void BuildTrade_WhenSignalNone_ShouldThrowArgumentException()
    {
        var strategy = new CryptoStrategy();

        Action act = () => strategy.BuildTrade(
            new KlineExecutionContext { MarketType = MarketType.CryptoSpot, Timeframe = "1h" },
            Klines,
            CreateIndicator(70m, 110m, 90m, 100m, 95m, 1m),
            SignalType.None);

        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void BuildTrade_WhenSignalLong_ShouldReturnTradePlan()
    {
        var strategy = new CryptoStrategy();
        var plan = strategy.BuildTrade(
            new KlineExecutionContext { MarketType = MarketType.CryptoSpot, Timeframe = "1h" },
            Klines,
            CreateIndicator(70m, 110m, 90m, 100m, 95m, 1m),
            SignalType.Long);

        plan.Side.Should().Be(SignalType.Long);
        plan.TakeProfit1.Should().BeGreaterThan(plan.Entry);
        plan.StopLoss.Should().BeLessThan(plan.Entry);
    }
}
