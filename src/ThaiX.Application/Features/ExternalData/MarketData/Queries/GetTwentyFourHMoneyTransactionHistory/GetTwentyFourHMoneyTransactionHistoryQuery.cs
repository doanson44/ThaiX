using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetTwentyFourHMoneyTransactionHistory;

/// <summary>
/// Reads previously-synced 24HMoney transaction history for a symbol from the local database
/// (populated daily by TwentyFourHMoneyTransactionSyncJob). When <see cref="CurrentPrice"/> is
/// supplied, also compares it against historical matched prices over several look-back windows.
/// </summary>
public sealed record GetTwentyFourHMoneyTransactionHistoryQuery : IAppQuery<TwentyFourHMoneyTransactionHistoryResponse>
{
    public required string Symbol { get; init; }

    /// <summary>When provided, computes <see cref="TwentyFourHMoneyTransactionHistoryResponse.PriceComparison"/>.</summary>
    public decimal? CurrentPrice { get; init; }

    public int Page { get; init; } = 1;
    public int PerPage { get; init; } = 50;
}

public sealed record TwentyFourHMoneyTransactionHistoryResponse
{
    public string Symbol { get; init; } = string.Empty;
    public decimal? CurrentPrice { get; init; }
    public int Page { get; init; }
    public int PerPage { get; init; }
    public int TotalCount { get; init; }
    public List<TwentyFourHMoneyTransactionRecordDto> Transactions { get; init; } = [];

    /// <summary>Null when <see cref="CurrentPrice"/> was not supplied or no history exists yet.</summary>
    public List<PriceComparisonPeriodDto>? PriceComparison { get; init; }

    public bool Success { get; init; } = true;
    public string? Message { get; init; }
}

public sealed record TwentyFourHMoneyTransactionRecordDto
{
    public DateOnly TradeDate { get; init; }
    public string TradeTime { get; init; } = string.Empty;
    public decimal Price { get; init; }
    public decimal Change { get; init; }
    public long MatchQuantity { get; init; }
    public long TotalVolume { get; init; }
    public string Side { get; init; } = string.Empty;
}

/// <summary>
/// Compares CurrentPrice against every matched order price within [FromDate, ToDate], volume-weighted.
/// "Above" means the matched price was higher than CurrentPrice;
/// "Below" is the opposite.
/// </summary>
public sealed record PriceComparisonPeriodDto
{
    /// <summary>"1D", "1W", "1M", "3M", "6M", or "1Y".</summary>
    public string Period { get; init; } = string.Empty;

    public DateOnly FromDate { get; init; }
    public DateOnly ToDate { get; init; }

    public long TransactionCount { get; init; }
    public long TotalVolume { get; init; }

    /// <summary>Matched volume bought at a price higher than CurrentPrice.</summary>
    public long BetterVolume { get; init; }

    /// <summary>Matched volume bought at a price lower than CurrentPrice.</summary>
    public long WorseVolume { get; init; }

    /// <summary>Matched volume bought at exactly CurrentPrice.</summary>
    public long EqualVolume { get; init; }

    /// <summary>% of volume in the period where CurrentPrice is better than the price paid.</summary>
    public decimal BetterPercent { get; init; }

    /// <summary>% of volume in the period where CurrentPrice is worse than the price paid.</summary>
    public decimal WorsePercent { get; init; }

    /// <summary>Volume-weighted average matched price in the period.</summary>
    public decimal AveragePrice { get; init; }

    /// <summary>Top 3 distinct highest prices in this period, each with summed volume.</summary>
    public List<PriceVolumePointDto> Top3Highest { get; init; } = [];

    /// <summary>Top 3 distinct lowest prices in this period, each with summed volume.</summary>
    public List<PriceVolumePointDto> Top3Lowest { get; init; } = [];
}

/// <summary>A price point with its aggregated matched volume.</summary>
public sealed record PriceVolumePointDto
{
    public decimal Price { get; init; }
    public long Volume { get; init; }
}
