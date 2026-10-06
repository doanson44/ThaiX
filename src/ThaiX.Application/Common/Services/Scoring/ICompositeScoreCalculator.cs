namespace ThaiX.Application.Common.Services.Scoring;

public interface ICompositeScoreCalculator
{
    CompositeScoreResult Calculate(int technicalScore, int portfolioScore, int eventPenalty, int liquidityPenalty);
}

public sealed record CompositeScoreResult
{
    public int CompositeScore { get; init; }
    public required string Level { get; init; }
}
