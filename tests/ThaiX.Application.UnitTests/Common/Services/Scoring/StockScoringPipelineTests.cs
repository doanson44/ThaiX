using Microsoft.Extensions.Options;
using ThaiX.Application.Common.Configuration;
using ThaiX.Application.Common.Services.Scoring;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectEvents;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectTechnicalSignals;

namespace ThaiX.Application.UnitTests.Common.Services.Scoring;

public sealed class StockScoringPipelineTests
{
    [Fact]
    public void Execute_WhenAllComponentsReturnValues_ComputesFinalScoreAndCompleteness()
    {
        var options = Options.Create(new LiquidityScoringOptions());
        var pipeline = new StockScoringPipeline(
            new TechnicalScoreCalculator(),
            new PortfolioScoreCalculator(),
            new EventRiskEvaluator(),
            new LiquidityPenaltyEvaluator(options),
            new CompositeScoreCalculator());

        var context = new StockScoringContext
        {
            Code = "AAA",
            TotalVolumeAvgCr20D = 90_000m,
            NmVolumeAvgCr20D = 10_000m,
            NmVolNmVolAvg20DPctCr = 20m,
            LongSignal = new VnDirectTechnicalSignalDto { TotalSignal = "STRONG BUY", Indicators = new List<VnDirectTechnicalIndicatorDto> { new() { Signal = "BUY" } } },
            ShortSignal = new VnDirectTechnicalSignalDto { TotalSignal = "SELL", Indicators = new List<VnDirectTechnicalIndicatorDto> { new() { Signal = "SELL" } } },
            IsInTcbsHoldings = true,
            DragonMetrics = new DragonHoldingMetrics(new HashSet<string> { "F1" }, 10m),
            Events = new List<VnDirectEventDto>
            {
                new() { Type = "alert", EffectiveDate = DateTime.UtcNow.AddDays(-2).ToString("yyyy-MM-dd") }
            },
            EventLookbackDays = 30
        };

        var result = pipeline.Execute(context);

        result.CompositeScore.Should().BeGreaterThan(0);
        result.EventPenalty.Should().BeNegative();
        result.LiquidityPenalty.Should().BeNegative();
        result.DataCompleteness.Should().Be(1.0m);
        result.ScoreBreakdown.Should().ContainSingle(item => item.Name == "TechnicalScore");
    }
}
