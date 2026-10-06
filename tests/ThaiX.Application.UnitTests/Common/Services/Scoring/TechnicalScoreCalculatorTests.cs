using ThaiX.Application.Common.Services.Scoring;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectTechnicalSignals;

namespace ThaiX.Application.UnitTests.Common.Services.Scoring;

public sealed class TechnicalScoreCalculatorTests
{
    [Fact]
    public void Calculate_WhenSignalsExist_ReturnsWeightedCompositeAndIndicatorCounts()
    {
        var calculator = new TechnicalScoreCalculator();
        var result = calculator.Calculate(
            new VnDirectTechnicalSignalDto
            {
                TotalSignal = "STRONG BUY",
                Indicators = new List<VnDirectTechnicalIndicatorDto>
                {
                    new() { Signal = "BUY" },
                    new() { Signal = "SELL" }
                }
            },
            new VnDirectTechnicalSignalDto
            {
                TotalSignal = "SELL",
                Indicators = new List<VnDirectTechnicalIndicatorDto>
                {
                    new() { Signal = "SELL" },
                    new() { Signal = "SELL" }
                }
            });

        result.LongSignal.Should().Be("STRONG BUY");
        result.ShortSignal.Should().Be("SELL");
        result.LongScore.Should().Be(70);
        result.ShortScore.Should().Be(14);
        result.CompositeScore.Should().Be(50);
        result.LongBuyCount.Should().Be(1);
        result.LongSellCount.Should().Be(1);
        result.ShortBuyCount.Should().Be(0);
        result.ShortSellCount.Should().Be(2);
    }
}
