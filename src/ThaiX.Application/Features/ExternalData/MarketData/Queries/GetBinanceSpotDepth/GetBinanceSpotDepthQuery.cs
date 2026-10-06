using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceSpotDepth;

/// <summary>
/// Query to retrieve Binance spot order book depth by symbol.
/// Limit is fixed in configuration.
/// </summary>
public sealed record GetBinanceSpotDepthQuery : IAppQuery<BinanceSpotDepthResponse>
{
    public string Symbol { get; init; } = string.Empty;
}

/// <summary>
/// Response containing Binance spot depth data.
/// </summary>
public sealed record BinanceSpotDepthResponse
{
    public required string Symbol { get; init; }
    public BinanceSpotDepthDataDto? Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Binance spot order book depth data.
/// asks/bids rows use [price, quantity].
/// </summary>
public sealed record BinanceSpotDepthDataDto
{
    public required List<List<decimal>> Asks { get; init; }
    public required List<List<decimal>> Bids { get; init; }
    public long LastUpdateId { get; init; }
}
