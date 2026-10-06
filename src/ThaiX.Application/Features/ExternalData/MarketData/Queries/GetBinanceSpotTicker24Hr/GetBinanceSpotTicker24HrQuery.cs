using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceSpotTicker24Hr;

/// <summary>
/// Query to retrieve Binance spot 24hr tickers for all symbols.
/// </summary>
public sealed record GetBinanceSpotTicker24HrQuery : IAppQuery<BinanceSpotTicker24HrResponse>;

/// <summary>
/// Response containing Binance spot 24hr ticker data.
/// </summary>
public sealed record BinanceSpotTicker24HrResponse
{
    public required List<BinanceSpotTickerDto> Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Binance spot 24hr ticker item.
/// </summary>
public sealed record BinanceSpotTickerDto
{
    public required string Symbol { get; init; }
    public decimal? PriceChange { get; init; }
    public decimal? PriceChangePercent { get; init; }
    public decimal? WeightedAvgPrice { get; init; }
    public decimal? PrevClosePrice { get; init; }
    public decimal? LastPrice { get; init; }
    public decimal? LastQty { get; init; }
    public decimal? BidPrice { get; init; }
    public decimal? BidQty { get; init; }
    public decimal? AskPrice { get; init; }
    public decimal? AskQty { get; init; }
    public decimal? OpenPrice { get; init; }
    public decimal? HighPrice { get; init; }
    public decimal? LowPrice { get; init; }
    public decimal? Volume { get; init; }
    public decimal? QuoteVolume { get; init; }
    public long OpenTime { get; init; }
    public long CloseTime { get; init; }
    public long FirstId { get; init; }
    public long LastId { get; init; }
    public long Count { get; init; }
}
