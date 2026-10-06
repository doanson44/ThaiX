using ThaiX.Application.Common.Services.Scoring;

namespace ThaiX.Application.UnitTests.Common.Services.Scoring;

public sealed class CompositeScoreCalculatorTests
{
    [Fact]
    public void Calculate_WhenCompositeIsNegative_ClampsToZeroAndReturnsCaution()
    {
        var calculator = new CompositeScoreCalculator();
        var result = calculator.Calculate(-10, 0, 0, 0);

        result.CompositeScore.Should().Be(0);
        result.Level.Should().Be("Caution");
    }

    [Fact]
    public void Calculate_WhenCompositeFallsInPositiveRange_ReturnsPositiveLevel()
    {
        var calculator = new CompositeScoreCalculator();
        var result = calculator.Calculate(60, 0, 0, 0);

        result.CompositeScore.Should().Be(60);
        result.Level.Should().Be("Positive");
    }

    [Fact]
    public void Calculate_WhenCompositeExceeds100_ClampsTo100AndReturnsStrong()
    {
        var calculator = new CompositeScoreCalculator();
        var result = calculator.Calculate(120, 0, 0, 0);

        result.CompositeScore.Should().Be(100);
        result.Level.Should().Be("Strong");
    }
}
