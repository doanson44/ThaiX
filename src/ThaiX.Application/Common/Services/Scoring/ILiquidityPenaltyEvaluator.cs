namespace ThaiX.Application.Common.Services.Scoring;

public interface ILiquidityPenaltyEvaluator
{
    /// <summary>Returns zero or negative points applied to composite score.</summary>
    int Evaluate(StockScoringContext context);
}
