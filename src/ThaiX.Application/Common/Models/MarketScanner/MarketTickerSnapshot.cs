namespace ThaiX.Application.Common.Models.MarketScanner;

public sealed record MarketTickerSnapshot
{
    public required string Symbol { get; init; }

    public required decimal Price { get; init; }

    public required decimal FundingRate { get; init; }

    public required decimal Volume24h { get; init; }

    public required DateTimeOffset CapturedAtUtc { get; init; }
}
