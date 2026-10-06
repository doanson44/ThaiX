using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceFuturesDepth;

/// <summary>
/// Query to retrieve Binance futures order book depth by symbol.
/// Limit is fixed in configuration.
/// </summary>
public sealed record GetBinanceFuturesDepthQuery : IAppQuery<BinanceFuturesDepthResponse>
{
    public string Symbol { get; init; } = string.Empty;
}

/// <summary>
/// Response containing Binance futures depth data.
/// </summary>
public sealed record BinanceFuturesDepthResponse
{
    public required string Symbol { get; init; }
    public BinanceFuturesDepthDataDto? Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Binance futures order book depth data.
/// asks/bids rows use [price, quantity].
/// </summary>
public sealed record BinanceFuturesDepthDataDto
{
    public required List<List<decimal>> Asks { get; init; }
    public required List<List<decimal>> Bids { get; init; }
    public long LastUpdateId { get; init; }
    public long EventTime { get; init; }
    public long TransactionTime { get; init; }
}
