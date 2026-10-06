using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24Hr;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24HrBySymbol;

/// <summary>
/// Query to retrieve MEXC spot 24h ticker statistics by symbol.
/// Example symbols: BTCUSDT, ETHUSDT.
/// </summary>
public sealed record GetMexcSpotTicker24HrBySymbolQuery : IAppQuery<MexcSpotTicker24HrBySymbolResponse>
{
    public string Symbol { get; init; } = string.Empty;
}

/// <summary>
/// Response containing MEXC spot 24h ticker data for one symbol.
/// </summary>
public sealed record MexcSpotTicker24HrBySymbolResponse
{
    public required string Symbol { get; init; }
    public MexcSpotTicker24HrDto? Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}
