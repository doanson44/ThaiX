using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetVnDirectTopStocks;

/// <summary>
/// Query to retrieve VnDirect top stocks list.
/// </summary>
public sealed record GetVnDirectTopStocksQuery : IAppQuery<VnDirectTopStocksResponse>, ICacheableQuery
{
    public bool IncludeEnrichment { get; init; } = true;
    public int MaxEventsLookbackDays { get; init; } = 30;

    public string CacheKey => CacheKeys.ExternalData.VnDirectTopStocks(IncludeEnrichment, MaxEventsLookbackDays);
    public TimeSpan? Expiration => TimeSpan.FromMinutes(15);
    public string CacheGroup => CacheGroups.ExternalData;
    public bool IsVersionedList => false;
}

/// <summary>
/// Response containing VnDirect top stocks data.
/// </summary>
public sealed record VnDirectTopStocksResponse
{
    public int CurrentPage { get; init; }
    public int Size { get; init; }
    public int TotalElements { get; init; }
    public int TotalPages { get; init; }
    public List<VnDirectTopStockDto> Data { get; init; } = [];
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// VnDirect top stock item.
/// </summary>
public sealed record VnDirectTopStockDto
{
    public string Code { get; init; } = string.Empty;
    public string Index { get; init; } = string.Empty;
    public decimal LastPrice { get; init; }
    public string LastUpdated { get; init; } = string.Empty;
    public decimal PriceChgCr1D { get; init; }
    public decimal PriceChgPctCr1D { get; init; }
    public decimal AccumulatedVal { get; init; }
    public decimal NmVolumeAvgCr20D { get; init; }
    public decimal NmVolNmVolAvg20DPctCr { get; init; }
    public decimal TotalVolumeAvgCr20D { get; init; }
    public decimal PtVolTotalVolAvg20DPctCr { get; init; }
    public decimal PtVolAvg5DTotalVolAvg20DPctCr { get; init; }
    public decimal PtVolSumCr5D { get; init; }
    public decimal PtValAvgCr5D { get; init; }
    public decimal PtVolAvgCr5D { get; init; }

    // Technical enrichment
    public string LongSignal { get; init; } = string.Empty;
    public string ShortSignal { get; init; } = string.Empty;
    public int LongBuyCount { get; init; }
    public int LongSellCount { get; init; }
    public int ShortBuyCount { get; init; }
    public int ShortSellCount { get; init; }

    // Portfolio enrichment
    public bool InTcbsCurrentHoldings { get; init; }
    public bool InTcbsAllTimeHoldings { get; init; }
    public int DragonFundCount { get; init; }
    public List<string> DragonFunds { get; init; } = [];
    public decimal DragonTotalWeight { get; init; }

    // Event enrichment
    public string EventRiskLevel { get; init; } = string.Empty;
    public int RecentEventCount { get; init; }
    public string MostSevereEventType { get; init; } = string.Empty;
    public string LatestEventEffectiveDate { get; init; } = string.Empty;

    // Scoring
    public int CompositeScore { get; init; }
    public int TechnicalScore { get; init; }
    public int PortfolioScore { get; init; }
    public int EventPenalty { get; init; }
    public int LiquidityPenalty { get; init; }
    public List<ScoreBreakdownItem> ScoreBreakdown { get; init; } = [];
    public bool DataCompleteness { get; init; }
}

public sealed record ScoreBreakdownItem
{
    public required string Name { get; init; }
    public required int Value { get; init; }
}
