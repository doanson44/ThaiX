using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBybitLinearTickers;

/// <summary>
/// Query to retrieve Bybit linear/perpetual market tickers.
/// Data is cached according to endpoint configuration.
/// </summary>
public sealed record GetBybitLinearTickersQuery : IAppQuery<BybitLinearTickersResponse>;

/// <summary>
/// Response containing Bybit linear/perpetual tickers data.
/// </summary>
public sealed record BybitLinearTickersResponse
{
    public required List<BybitLinearTickerDto> Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Bybit linear/perpetual ticker information.
/// </summary>
public sealed record BybitLinearTickerDto
{
    public required string Symbol { get; init; }
    public string? LastPrice { get; init; }
    public string? IndexPrice { get; init; }
    public string? MarkPrice { get; init; }
    public string? Price24hPcnt { get; init; }
    public string? HighPrice24h { get; init; }
    public string? LowPrice24h { get; init; }
    public string? Volume24h { get; init; }
    public string? Turnover24h { get; init; }
    public string? OpenInterest { get; init; }
    public string? FundingRate { get; init; }
    public string? NextFundingTime { get; init; }
}
