using System.Text.Json;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractTickers;

/// <summary>
/// Query to retrieve MEXC contract ticker data for all market symbols.
/// </summary>
public sealed record GetMexcContractTickersQuery : IAppQuery<MexcContractTickersResponse>;

/// <summary>
/// Response containing MEXC contract ticker data for all symbols.
/// </summary>
public sealed record MexcContractTickersResponse
{
    public required List<MexcContractTickerDto> Data { get; init; }
    public string? Message { get; init; }
    public bool Success { get; init; }
}

/// <summary>
/// MEXC contract ticker information.
/// </summary>
public sealed record MexcContractTickerDto
{
    public int? ContractId { get; init; }
    public required string Symbol { get; init; }
    public decimal? LastPrice { get; init; }
    public decimal? Bid1 { get; init; }
    public decimal? Ask1 { get; init; }
    public decimal? High24Price { get; init; }
    public decimal? Low24Price { get; init; }
    public decimal? Volume24 { get; init; }
    public decimal? Amount24 { get; init; }
    public decimal? HoldVol { get; init; }
    public decimal? RiseFallRate { get; init; }
    public decimal? RiseFallValue { get; init; }
    public decimal? IndexPrice { get; init; }
    public decimal? FairPrice { get; init; }
    public decimal? FundingRate { get; init; }
    public decimal? MaxBidPrice { get; init; }
    public decimal? MinAskPrice { get; init; }
    public JsonElement? RiseFallRates { get; init; }
    public List<decimal>? RiseFallRatesOfTimezone { get; init; }
    public long? Timestamp { get; init; }
    public decimal? CompositeScore { get; init; }
    public MexcScoreBreakdownDto? ScoreBreakdown { get; init; }
}

/// <summary>
/// Breakdown of the composite score components for a MEXC contract ticker.
/// </summary>
public sealed record MexcScoreBreakdownDto
{
    /// <summary>0-25: percentile rank of 24h USD volume among all MEXC contract tickers.</summary>
    public decimal VolumeScore { get; init; }
    /// <summary>0-20: percentile rank of open interest (HoldVol) among all MEXC contract tickers.</summary>
    public decimal OpenInterestScore { get; init; }
    /// <summary>0-15: funding rate neutrality; closest to 0 scores highest.</summary>
    public decimal FundingScore { get; init; }
    /// <summary>0-10: 24h momentum from RiseFallRate, clamped to [-5%, +5%].</summary>
    public decimal MomentumScore { get; init; }
    /// <summary>0-15: blend of ChainBroker BrokerScore, SecurityScore, and Rank tier.</summary>
    public decimal BrokerQualityScore { get; init; }
    /// <summary>0-5: Twitter engagement percentile among MEXC-matched ChainBroker projects.</summary>
    public decimal SocialScore { get; init; }
    /// <summary>0-5: log-normalized total fundraising (cap $50M = full 5pts).</summary>
    public decimal FundraisingScore { get; init; }
    /// <summary>0-5: circulating supply health; higher circulation = less sell-pressure risk.</summary>
    public decimal SupplyHealthScore { get; init; }
    /// <summary>0 to -15: penalty for token unlock events within the next 14 days.</summary>
    public decimal UnlockPenalty { get; init; }
    /// <summary>0 to -5: penalty when FDV is significantly larger than market cap (future dilution risk).</summary>
    public decimal FdvOverhangPenalty { get; init; }
}
