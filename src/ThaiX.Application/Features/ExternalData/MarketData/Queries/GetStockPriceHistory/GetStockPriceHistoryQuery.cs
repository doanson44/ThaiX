using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetStockPriceHistory;

/// <summary>
/// Query to retrieve stock price history from CafeF by stock symbol.
/// Symbol examples: VNM, VIC, NKG.
/// </summary>
public sealed record GetStockPriceHistoryQuery : IAppQuery<StockPriceHistoryResponse>
{
    public required string Symbol { get; init; }
}

/// <summary>
/// Response containing stock price history entries from CafeF.
/// </summary>
public sealed record StockPriceHistoryResponse
{
    public required string Symbol { get; init; }
    public required List<StockPriceHistoryItemDto> Data { get; init; }
    public bool Success { get; init; }
    public string? Message { get; init; }
}

/// <summary>
/// One stock price history item for a trading date.
/// </summary>
public sealed record StockPriceHistoryItemDto
{
    public required string Symbol { get; init; }
    public DateTime TradeDate { get; init; }
    public decimal BasicPrice { get; init; }
    public decimal OpenPrice { get; init; }
    public decimal HighPrice { get; init; }
    public decimal LowPrice { get; init; }
    public decimal ClosePrice { get; init; }
    public decimal Volume { get; init; }
    public decimal TotalValue { get; init; }
}
