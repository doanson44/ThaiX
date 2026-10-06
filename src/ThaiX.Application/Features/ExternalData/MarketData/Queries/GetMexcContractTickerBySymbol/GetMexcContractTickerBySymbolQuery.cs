using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractTickers;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractTickerBySymbol;

/// <summary>
/// Query to retrieve MEXC contract ticker data by symbol.
/// Example symbols: BTC_USDT, ETH_USDT.
/// </summary>
public sealed record GetMexcContractTickerBySymbolQuery : IAppQuery<MexcContractTickerBySymbolResponse>
{
    public string Symbol { get; init; } = string.Empty;
}

/// <summary>
/// Response containing MEXC contract ticker data for a specific symbol.
/// </summary>
public sealed record MexcContractTickerBySymbolResponse
{
    public required string Symbol { get; init; }
    public MexcContractTickerDto? Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}
