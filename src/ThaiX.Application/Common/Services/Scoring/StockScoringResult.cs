using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectTopStocks;

namespace ThaiX.Application.Common.Services.Scoring;

public sealed record StockScoringResult
{
    public required string LongSignal { get; init; }
    public required string ShortSignal { get; init; }
    public required int LongBuyCount { get; init; }
    public required int LongSellCount { get; init; }
    public required int ShortBuyCount { get; init; }
    public required int ShortSellCount { get; init; }
    public required int TechnicalScore { get; init; }
    public required int PortfolioScore { get; init; }
    public required int EventPenalty { get; init; }
    public required int LiquidityPenalty { get; init; }
    public required int CompositeScore { get; init; }
    public required string EventRiskLevel { get; init; }
    public required int RecentEventCount { get; init; }
    public required string MostSevereEventType { get; init; }
    public required string LatestEventEffectiveDate { get; init; }
    public required decimal DataCompleteness { get; init; }
    public required List<ScoreBreakdownItem> ScoreBreakdown { get; init; } = [];
}
