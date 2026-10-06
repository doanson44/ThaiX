using ThaiX.Application.Common.Services.Scoring;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectEvents;

namespace ThaiX.Application.UnitTests.Common.Services.Scoring;

public sealed class EventRiskEvaluatorTests
{
    [Fact]
    public void Evaluate_WhenNoEvents_ReturnsNoneResult()
    {
        var evaluator = new EventRiskEvaluator();
        var result = evaluator.Evaluate(Array.Empty<VnDirectEventDto>(), 30);

        result.Level.Should().Be("None");
        result.Penalty.Should().Be(0);
        result.MostSevereType.Should().BeEmpty();
        result.LatestEffectiveDate.Should().BeEmpty();
        result.Count.Should().Be(0);
    }

    [Fact]
    public void Evaluate_WhenMultipleEvents_ReturnsMostSevereAndCumulativePenalty()
    {
        var evaluator = new EventRiskEvaluator();
        var now = DateTime.UtcNow;
        var events = new[]
        {
            new VnDirectEventDto
            {
                Code = "ABC",
                Type = "halt",
                EffectiveDate = now.AddDays(-2).ToString("yyyy-MM-dd")
            },
            new VnDirectEventDto
            {
                Code = "ABC",
                Type = "nomargin",
                DisclosureDate = now.AddDays(-1).ToString("yyyy-MM-dd")
            }
        };

        var result = evaluator.Evaluate(events, 30);

        result.Level.Should().Be("Severe");
        result.Penalty.Should().Be(-35);
        result.MostSevereType.Should().Be("halt");
        result.LatestEffectiveDate.Should().Be(now.AddDays(-1).ToString("yyyy-MM-dd"));
        result.Count.Should().Be(2);
    }
}
