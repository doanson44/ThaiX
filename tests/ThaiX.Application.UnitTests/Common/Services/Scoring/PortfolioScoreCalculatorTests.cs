using ThaiX.Application.Common.Services.Scoring;

namespace ThaiX.Application.UnitTests.Common.Services.Scoring;

public sealed class PortfolioScoreCalculatorTests
{
    [Fact]
    public void Calculate_WhenInTcbsHoldingsAndDragonMetricsReturnMaxScore()
    {
        var calculator = new PortfolioScoreCalculator();
        var result = calculator.Calculate(true, new DragonHoldingMetrics(new HashSet<string> { "F1", "F2", "F3", "F4" }, 15m));

        result.TcbsScore.Should().Be(12);
        result.DragonScore.Should().Be(18);
        result.CompositeScore.Should().Be(30);
    }

    [Fact]
    public void Calculate_WhenNotInTcbsHoldingsAndNoDragonFunds_ReturnsZero()
    {
        var calculator = new PortfolioScoreCalculator();
        var result = calculator.Calculate(false, new DragonHoldingMetrics(new HashSet<string>(), 0m));

        result.TcbsScore.Should().Be(0);
        result.DragonScore.Should().Be(0);
        result.CompositeScore.Should().Be(0);
    }
}
