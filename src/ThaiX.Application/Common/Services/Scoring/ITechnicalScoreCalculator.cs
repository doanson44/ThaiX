using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectTechnicalSignals;

namespace ThaiX.Application.Common.Services.Scoring;

public interface ITechnicalScoreCalculator
{
    TechnicalScoreResult Calculate(VnDirectTechnicalSignalDto? longSignal, VnDirectTechnicalSignalDto? shortSignal);
}

public sealed record TechnicalScoreResult
{
    public required string LongSignal { get; init; }
    public required string ShortSignal { get; init; }
    public int LongScore { get; init; }
    public int ShortScore { get; init; }
    public int CompositeScore { get; init; }
    public int LongBuyCount { get; init; }
    public int LongSellCount { get; init; }
    public int ShortBuyCount { get; init; }
    public int ShortSellCount { get; init; }
}
