using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceFuturesTicker24Hr;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceFuturesTicker24HrBySymbol;

/// <summary>
/// Query to retrieve Binance futures 24hr ticker by symbol.
/// </summary>
public sealed record GetBinanceFuturesTicker24HrBySymbolQuery : IAppQuery<BinanceFuturesTicker24HrBySymbolResponse>
{
    public required string Symbol { get; init; }
}

/// <summary>
/// Response containing Binance futures 24hr ticker by symbol.
/// </summary>
public sealed record BinanceFuturesTicker24HrBySymbolResponse
{
    public required string Symbol { get; init; }
    public BinanceFuturesTickerDto? Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}
