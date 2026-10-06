using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetBinanceFundingRates;

/// <summary>
/// Query to retrieve Binance futures funding rate history.
/// </summary>
public sealed record GetBinanceFundingRatesQuery : IAppQuery<BinanceFundingRatesResponse>;

/// <summary>
/// Response containing Binance funding rates.
/// </summary>
public sealed record BinanceFundingRatesResponse
{
    public required List<BinanceFundingRateDto> Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// Binance funding rate item.
/// </summary>
public sealed record BinanceFundingRateDto
{
    public required string Symbol { get; init; }
    public decimal FundingRate { get; init; }
    public long FundingTime { get; init; }
    public decimal? MarkPrice { get; init; }
}
