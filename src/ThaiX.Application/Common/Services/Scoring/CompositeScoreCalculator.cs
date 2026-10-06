namespace ThaiX.Application.Common.Services.Scoring;

public sealed class CompositeScoreCalculator : ICompositeScoreCalculator
{
    public CompositeScoreResult Calculate(int technicalScore, int portfolioScore, int eventPenalty, int liquidityPenalty)
    {
        var composite = Clamp(technicalScore + portfolioScore + eventPenalty + liquidityPenalty, 0, 100);
        return new CompositeScoreResult
        {
            CompositeScore = composite,
            Level = GetCompositeLevel(composite)
        };
    }

    private static int Clamp(int value, int min, int max)
        => value < min ? min : value > max ? max : value;

    private static string GetCompositeLevel(int score)
        => score switch
        {
            >= 75 => "Strong",
            >= 55 => "Positive",
            >= 40 => "Neutral",
            _ => "Caution"
        };
}
