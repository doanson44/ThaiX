using ThaiX.Application.Strategies;
using ThaiX.Application.Trading;

namespace ThaiX.Application.UnitTests.Features.Strategies;

public sealed class KlineTrendHelperTests
{
    [Fact]
    public void DetectTrend_WhenEmaAlignmentUp_ShouldReturnUp()
    {
        var result = KlineTrendHelper.DetectTrend(new IndicatorResult
        {
            Ema9 = 10m,
            Ema21 = 8m,
            Ema50 = 5m,
            Rsi14 = 0m,
            Atr14 = 0m,
            AverageVolume20 = 0m,
            LatestClose = 0m,
            HighestHigh20 = 0m,
            LowestLow20 = 0m
        });

        result.Should().Be(TrendDirection.Up);
    }

    [Fact]
    public void DetectTrend_WhenEmaAlignmentDown_ShouldReturnDown()
    {
        var result = KlineTrendHelper.DetectTrend(new IndicatorResult
        {
            Ema9 = 5m,
            Ema21 = 7m,
            Ema50 = 10m,
            Rsi14 = 0m,
            Atr14 = 0m,
            AverageVolume20 = 0m,
            LatestClose = 0m,
            HighestHigh20 = 0m,
            LowestLow20 = 0m
        });

        result.Should().Be(TrendDirection.Down);
    }

    [Fact]
    public void DetectTrend_WhenEmaNotAligned_ShouldReturnSideways()
    {
        var result = KlineTrendHelper.DetectTrend(new IndicatorResult
        {
            Ema9 = 10m,
            Ema21 = 5m,
            Ema50 = 8m,
            Rsi14 = 0m,
            Atr14 = 0m,
            AverageVolume20 = 0m,
            LatestClose = 0m,
            HighestHigh20 = 0m,
            LowestLow20 = 0m
        });

        result.Should().Be(TrendDirection.Sideways);
    }
}
