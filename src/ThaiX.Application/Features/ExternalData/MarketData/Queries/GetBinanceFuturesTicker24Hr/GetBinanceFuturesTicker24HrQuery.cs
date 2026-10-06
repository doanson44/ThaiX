using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceFuturesTicker24Hr;

/// <summary>
/// Query to retrieve Binance futures 24hr tickers for all symbols.
/// </summary>
public sealed record GetBinanceFuturesTicker24HrQuery : IAppQuery<BinanceFuturesTicker24HrResponse>;

/// <summary>
/// Response containing Binance futures 24hr ticker data.
/// </summary>
public sealed record BinanceFuturesTicker24HrResponse
{
    public required List<BinanceFuturesTickerDto> Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Binance futures 24hr ticker item.
/// </summary>
public sealed record BinanceFuturesTickerDto
{
    public required string Symbol { get; init; }
    public decimal? PriceChange { get; init; }
    public decimal? PriceChangePercent { get; init; }
    public decimal? WeightedAvgPrice { get; init; }
    public decimal? LastPrice { get; init; }
    public decimal? LastQty { get; init; }
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
