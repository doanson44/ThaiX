using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24Hr;

/// <summary>
/// Query to retrieve MEXC spot 24h ticker statistics for all symbols.
/// </summary>
public sealed record GetMexcSpotTicker24HrQuery : IAppQuery<MexcSpotTicker24HrResponse>;

/// <summary>
/// Response containing MEXC spot 24h ticker statistics.
/// </summary>
public sealed record MexcSpotTicker24HrResponse
{
    public required List<MexcSpotTicker24HrDto> Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// MEXC spot 24h ticker item.
/// </summary>
public sealed record MexcSpotTicker24HrDto
{
    public required string Symbol { get; init; }
    public string? PriceChange { get; init; }
    public string? PriceChangePercent { get; init; }
    public string? PrevClosePrice { get; init; }
    public string? LastPrice { get; init; }
    public string? BidPrice { get; init; }
    public string? BidQty { get; init; }
    public string? AskPrice { get; init; }
    public string? AskQty { get; init; }
    public string? OpenPrice { get; init; }
    public string? HighPrice { get; init; }
    public string? LowPrice { get; init; }
    public string? Volume { get; init; }
    public string? QuoteVolume { get; init; }
    public long OpenTime { get; init; }
    public long CloseTime { get; init; }
    public long? Count { get; init; }
    public decimal? CompositeScore { get; init; }
    public MexcSpotScoreBreakdownDto? ScoreBreakdown { get; init; }
}

/// <summary>
/// Score breakdown for a MEXC spot ticker (no funding rate or open interest).
/// </summary>
public sealed record MexcSpotScoreBreakdownDto
{
    /// <summary>0-35: percentile rank of 24h USD quote volume.</summary>
    public decimal VolumeScore { get; init; }
    /// <summary>0-15: 24h price change momentum, clamped to +-5%.</summary>
    public decimal MomentumScore { get; init; }
    /// <summary>0-20: blend of ChainBroker BrokerScore, SecurityScore, and Rank tier.</summary>
    public decimal BrokerQualityScore { get; init; }
    /// <summary>0-10: Twitter engagement percentile among MEXC-matched projects.</summary>
    public decimal SocialScore { get; init; }
    /// <summary>0-10: log-normalised total fundraising amount.</summary>
    public decimal FundraisingScore { get; init; }
    /// <summary>0-10: percent circulating supply health.</summary>
    public decimal SupplyHealthScore { get; init; }
    /// <summary>0 to -15: upcoming token unlock penalty within 14 days.</summary>
    public decimal UnlockPenalty { get; init; }
    /// <summary>0 to -5: FDV / market cap overhang penalty.</summary>
    public decimal FdvOverhangPenalty { get; init; }
    /// <summary>0 to -10: private sale ROI overhang (high private ROI = early investors in large profit = sell pressure).</summary>
    public decimal PrivateSaleOverhangPenalty { get; init; }
    /// <summary>0 to -5: public sale (IDO/IEO) ROI overhang; smaller allocation but still creates sell pressure.</summary>
    public decimal PublicSaleOverhangPenalty { get; init; }
    /// <summary>0 to -3: token listed less than 6 months ago; early vesting cliff increases dump risk.</summary>
    public decimal NewListingPenalty { get; init; }
    /// <summary>0 to +5: number of known VC fund backers from ChainBroker data.</summary>
    public decimal VcBackingBonus { get; init; }
}
