namespace ThaiX.Application.Features.Trading.Queries.GetTradeSuggestion;

public sealed record TradeSuggestionDto
{
    public required string Trend { get; init; }
    public required string Momentum { get; init; }
    public required string Setup { get; init; }
    public required string Signal { get; init; }

    public decimal? EntryPrice { get; init; }
    public decimal? StopLoss { get; init; }
    public decimal? TakeProfit1 { get; init; }
    public decimal? TakeProfit2 { get; init; }

    public decimal Confidence { get; init; }
}
