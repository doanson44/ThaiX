using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractFundingRates;

/// <summary>
/// Query to retrieve MEXC contract funding rates for all symbols.
/// </summary>
public sealed record GetMexcContractFundingRatesQuery : IAppQuery<MexcContractFundingRatesResponse>;

/// <summary>
/// Response containing MEXC contract funding rates.
/// </summary>
public sealed record MexcContractFundingRatesResponse
{
    public required List<MexcContractFundingRateDto> Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// MEXC contract funding rate item.
/// </summary>
public sealed record MexcContractFundingRateDto
{
    public required string Symbol { get; init; }
    public decimal FundingRate { get; init; }
    public decimal MaxFundingRate { get; init; }
    public decimal MinFundingRate { get; init; }
    public int CollectCycle { get; init; }
    public long NextSettleTime { get; init; }
    public long Timestamp { get; init; }
}
