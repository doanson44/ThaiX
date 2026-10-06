namespace ThaiX.Application.Common.Services.Scoring;

public interface IPortfolioScoreCalculator
{
    PortfolioScoreResult Calculate(bool isInTcbsHoldings, DragonHoldingMetrics dragonHoldingMetrics);
}

public sealed record DragonHoldingMetrics(HashSet<string> Funds, decimal TotalWeight);

/// <summary>
/// Breakdown of portfolio score components for transparency in ScoreBreakdown.
/// </summary>
public sealed record PortfolioScoreResult
{
    public int TcbsScore { get; init; }
    public int DragonScore { get; init; }
    public int CompositeScore { get; init; }
}