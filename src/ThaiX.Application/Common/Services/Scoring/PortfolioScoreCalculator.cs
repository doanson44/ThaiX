namespace ThaiX.Application.Common.Services.Scoring;

public sealed class PortfolioScoreCalculator : IPortfolioScoreCalculator
{
    private const int TcbsHoldingScore = 12;
    private const int DragonFundScorePerFund = 4;
    private const int MaxDragonWeightBonus = 6;
    private const int MaxDragonScore = 18;

    // Max PortfolioScore = TcbsHoldingScore(12) + MaxDragonScore(18) = 30

    public PortfolioScoreResult Calculate(bool isInTcbsHoldings, DragonHoldingMetrics dragonHoldingMetrics)
    {
        var tcbsScore = isInTcbsHoldings ? TcbsHoldingScore : 0;

        var weightBonus = (int)Math.Min(MaxDragonWeightBonus,
            Math.Round(dragonHoldingMetrics.TotalWeight / 5m, MidpointRounding.AwayFromZero));
        var dragonScore = Math.Min(MaxDragonScore,
            (dragonHoldingMetrics.Funds.Count * DragonFundScorePerFund) + weightBonus);

        return new PortfolioScoreResult
        {
            TcbsScore = tcbsScore,
            DragonScore = dragonScore,
            CompositeScore = tcbsScore + dragonScore
        };
    }
}
