using Microsoft.Extensions.Options;
using ThaiX.Application.Common.Configuration;
using ThaiX.Application.Common.Services.Scoring;

namespace ThaiX.Application.UnitTests.Common.Services.Scoring;

public sealed class LiquidityPenaltyEvaluatorTests
{
    [Fact]
    public void Evaluate_WhenDisabled_ReturnsZero()
    {
        var options = Options.Create(new LiquidityScoringOptions { Enabled = false });
        var evaluator = new LiquidityPenaltyEvaluator(options);

        var result = evaluator.Evaluate(new StockScoringContext
        {
            Code = "AAA",
            TotalVolumeAvgCr20D = 0m,
            NmVolumeAvgCr20D = 0m,
            NmVolNmVolAvg20DPctCr = 0m,
            LongSignal = null,
            ShortSignal = null,
            IsInTcbsHoldings = false,
            DragonMetrics = new DragonHoldingMetrics(new(), 0m),
            Events = Array.Empty<Application.Features.ExternalData.MarketData.Queries.GetVnDirectEvents.VnDirectEventDto>(),
            EventLookbackDays = 0
        });

        result.Should().Be(0);
    }

    [Fact]
    public void Evaluate_WhenModerateVolumeAndRelativeNmVolumeIsPoor_ReturnsExtraPenaltyApplied()
    {
        var options = Options.Create(new LiquidityScoringOptions());
        var evaluator = new LiquidityPenaltyEvaluator(options);

        var result = evaluator.Evaluate(new StockScoringContext
        {
            Code = "AAA",
            TotalVolumeAvgCr20D = 90_000m,
            NmVolumeAvgCr20D = 10_000m,
            NmVolNmVolAvg20DPctCr = 20m,
            LongSignal = null,
            ShortSignal = null,
            IsInTcbsHoldings = false,
            DragonMetrics = new DragonHoldingMetrics(new(), 0m),
            Events = Array.Empty<Application.Features.ExternalData.MarketData.Queries.GetVnDirectEvents.VnDirectEventDto>(),
            EventLookbackDays = 0
        });

        result.Should().Be(-14);
    }
}
