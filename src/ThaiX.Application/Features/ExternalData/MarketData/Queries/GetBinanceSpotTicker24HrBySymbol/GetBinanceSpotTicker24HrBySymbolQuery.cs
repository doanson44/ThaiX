using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceSpotTicker24Hr;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceSpotTicker24HrBySymbol;

/// <summary>
/// Query to retrieve Binance spot 24hr ticker by symbol.
/// </summary>
public sealed record GetBinanceSpotTicker24HrBySymbolQuery : IAppQuery<BinanceSpotTicker24HrBySymbolResponse>
{
    public required string Symbol { get; init; }
}

/// <summary>
/// Response containing Binance spot 24hr ticker by symbol.
/// </summary>
public sealed record BinanceSpotTicker24HrBySymbolResponse
{
    public required string Symbol { get; init; }
    public BinanceSpotTickerDto? Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}
