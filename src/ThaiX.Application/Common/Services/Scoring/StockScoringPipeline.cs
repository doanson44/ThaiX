using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectTopStocks;

namespace ThaiX.Application.Common.Services.Scoring;

public sealed class StockScoringPipeline : IStockScoringPipeline
{
    private readonly ITechnicalScoreCalculator _technicalScoreCalculator;
    private readonly IPortfolioScoreCalculator _portfolioScoreCalculator;
    private readonly IEventRiskEvaluator _eventRiskEvaluator;
    private readonly ILiquidityPenaltyEvaluator _liquidityPenaltyEvaluator;
    private readonly ICompositeScoreCalculator _compositeScoreCalculator;

    public StockScoringPipeline(
        ITechnicalScoreCalculator technicalScoreCalculator,
        IPortfolioScoreCalculator portfolioScoreCalculator,
        IEventRiskEvaluator eventRiskEvaluator,
        ILiquidityPenaltyEvaluator liquidityPenaltyEvaluator,
        ICompositeScoreCalculator compositeScoreCalculator)
    {
        _technicalScoreCalculator = technicalScoreCalculator;
        _portfolioScoreCalculator = portfolioScoreCalculator;
        _eventRiskEvaluator = eventRiskEvaluator;
        _liquidityPenaltyEvaluator = liquidityPenaltyEvaluator;
        _compositeScoreCalculator = compositeScoreCalculator;
    }

    public StockScoringResult Execute(StockScoringContext context)
    {
        var technicalResult = _technicalScoreCalculator.Calculate(context.LongSignal, context.ShortSignal);
        var portfolioResult = _portfolioScoreCalculator.Calculate(context.IsInTcbsHoldings, context.DragonMetrics);
        var eventRisk = _eventRiskEvaluator.Evaluate(context.Events, context.EventLookbackDays);
        var liquidityPenalty = _liquidityPenaltyEvaluator.Evaluate(context);
        var compositeResult = _compositeScoreCalculator.Calculate(
            technicalResult.CompositeScore,
            portfolioResult.CompositeScore,
            eventRisk.Penalty,
            liquidityPenalty);

        // DataCompleteness measures technical signal coverage only.
        // Portfolio absence and empty events are valid fetched states — not having holdings
        // is real data, not missing data. Each signal contributes 0.5 → range [0.0, 1.0].
        var completeness =
            (context.LongSignal is not null ? 0.5m : 0m) +
            (context.ShortSignal is not null ? 0.5m : 0m);

        // Granular breakdown for UI transparency
        var scoreBreakdown = new List<ScoreBreakdownItem>
        {
            new() { Name = "LongSignalScore",  Value = technicalResult.LongScore },
            new() { Name = "ShortSignalScore", Value = technicalResult.ShortScore },
            new() { Name = "TechnicalScore",   Value = technicalResult.CompositeScore },
            new() { Name = "TcbsScore",        Value = portfolioResult.TcbsScore },
            new() { Name = "DragonScore",      Value = portfolioResult.DragonScore },
            new() { Name = "PortfolioScore",   Value = portfolioResult.CompositeScore },
            new() { Name = "EventPenalty",     Value = eventRisk.Penalty },
            new() { Name = "LiquidityPenalty", Value = liquidityPenalty }
        };

        return new StockScoringResult
        {
            LongSignal = technicalResult.LongSignal,
            ShortSignal = technicalResult.ShortSignal,
            LongBuyCount = technicalResult.LongBuyCount,
            LongSellCount = technicalResult.LongSellCount,
            ShortBuyCount = technicalResult.ShortBuyCount,
            ShortSellCount = technicalResult.ShortSellCount,
            TechnicalScore = technicalResult.CompositeScore,
            PortfolioScore = portfolioResult.CompositeScore,
            EventPenalty = eventRisk.Penalty,
            LiquidityPenalty = liquidityPenalty,
            CompositeScore = compositeResult.CompositeScore,
            EventRiskLevel = eventRisk.Level,
            RecentEventCount = eventRisk.Count,
            MostSevereEventType = eventRisk.MostSevereType,
            LatestEventEffectiveDate = eventRisk.LatestEffectiveDate,
            DataCompleteness = completeness,
            ScoreBreakdown = scoreBreakdown
        };
    }
}
