using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBybitSpotTickers;

/// <summary>
/// Query to retrieve Bybit spot market tickers.
/// Data is cached according to endpoint configuration.
/// </summary>
public sealed record GetBybitSpotTickersQuery : IAppQuery<BybitSpotTickersResponse>;

/// <summary>
/// Response containing Bybit spot tickers data.
/// </summary>
public sealed record BybitSpotTickersResponse
{
    public required List<BybitSpotTickerDto> Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Bybit spot ticker information.
/// </summary>
public sealed record BybitSpotTickerDto
{
    public required string Symbol { get; init; }
    public string? LastPrice { get; init; }
    public string? Bid1Price { get; init; }
    public string? Ask1Price { get; init; }
    public string? Price24hPcnt { get; init; }
    public string? HighPrice24h { get; init; }
    public string? LowPrice24h { get; init; }
    public string? Volume24h { get; init; }
    public string? Turnover24h { get; init; }
    public string? UsdIndexPrice { get; init; }
}
