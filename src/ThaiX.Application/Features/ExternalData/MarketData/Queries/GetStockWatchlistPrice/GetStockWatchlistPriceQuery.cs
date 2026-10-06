using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetStockWatchlistPrice;

/// <summary>
/// Query to retrieve CafeF watchlist price snapshot by stock symbol.
/// Symbol examples: VNM, VIC, NKG.
/// </summary>
public sealed record GetStockWatchlistPriceQuery : IAppQuery<StockWatchlistPriceResponse>
{
    public required string Symbol { get; init; }
}

/// <summary>
/// Response containing watchlist price snapshot for one symbol.
/// </summary>
public sealed record StockWatchlistPriceResponse
{
    public required string Symbol { get; init; }
    public DateTimeOffset? LastTradeDate { get; init; }
    public decimal Price { get; init; }
    public decimal RefPrice { get; init; }
    public decimal FloorPrice { get; init; }
    public decimal CeilingPrice { get; init; }
    public decimal Volume { get; init; }
    public decimal HighPrice { get; init; }
    public decimal LowPrice { get; init; }
    public bool Success { get; init; }
    public string? Message { get; init; }
}
