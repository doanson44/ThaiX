using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractFundingRateHistory;

/// <summary>
/// Query to retrieve settled MEXC contract funding rates for one symbol.
/// </summary>
public sealed record GetMexcContractFundingRateHistoryQuery : IAppQuery<MexcContractFundingRateHistoryResponse>
{
    public string Symbol { get; init; } = string.Empty;
    public int PageNum { get; init; } = 1;
    public int PageSize { get; init; } = 20;
}

/// <summary>
/// Response containing one page of MEXC contract funding rate history.
/// </summary>
public sealed record MexcContractFundingRateHistoryResponse
{
    public required string Symbol { get; init; }
    public required int PageNum { get; init; }
    public required int PageSize { get; init; }
    public int TotalCount { get; init; }
    public int TotalPage { get; init; }
    public int CurrentPage { get; init; }
    public required List<MexcContractFundingRateHistoryItemDto> Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// One settled MEXC contract funding rate.
/// </summary>
public sealed record MexcContractFundingRateHistoryItemDto
{
    public required string Symbol { get; init; }
    public decimal FundingRate { get; init; }
    public long SettleTime { get; init; }
    public int CollectCycle { get; init; }
}
