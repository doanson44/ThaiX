using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcContractTickers;

/// <summary>
/// Handler for GetMexcContractTickersQuery.
/// Retrieves MEXC contract ticker data for all market symbols, enriches each ticker with a
/// composite score computed from live market metrics and ChainBroker project quality data.
/// </summary>
public sealed class GetMexcContractTickersQueryHandler
    : IRequestHandler<GetMexcContractTickersQuery, MexcContractTickersResponse>
{
    private readonly IExternalDataService _externalDataService;
    private readonly IApplicationDbContext _dbContext;

    public GetMexcContractTickersQueryHandler(
        IExternalDataService externalDataService,
        IApplicationDbContext dbContext)
    {
        _externalDataService = externalDataService;
        _dbContext = dbContext;
    }

    public async Task<MexcContractTickersResponse> Handle(
        GetMexcContractTickersQuery request,
        CancellationToken cancellationToken)
    {
        // Start MEXC API call immediately; run DB queries concurrently while waiting.
        var apiTask = _externalDataService
            .GetMexcContractTickerAsync<InternalMexcApiArrayResponse>(cancellationToken);

        var projectsRaw = await _dbContext.ChainBrokerProjects
            .Where(p => p.Ticker != null)
            .Select(p => new
            {
                p.Ticker,
                p.BrokerScore,
                p.SecurityScore,
                p.TwitterScore,
                p.Rank,
                p.TotalRaiseUsd,
                p.PercentCirculating,
                p.MarketCapUsd,
                p.FdmcUsd
            })
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cutoff = today.AddDays(14);

        var unlocksRaw = await _dbContext.ChainBrokerUnlocks
            .Where(u => u.Ticker != null
                     && u.NextUnlockDate.HasValue
                     && u.NextUnlockDate >= today
                     && u.NextUnlockDate <= cutoff)
            .Select(u => new { u.Ticker, u.UnlockPercent })
            .ToListAsync(cancellationToken);

        var apiResponse = await apiTask;

        if (apiResponse is null || !apiResponse.Success || apiResponse.Code != 0 || apiResponse.Data is null)
        {
            return new MexcContractTickersResponse
            {
                Data = [],
                Success = false,
                Message = "Failed to retrieve MEXC contract tickers from external API."
            };
        }

        var validTickers = apiResponse.Data
            .Where(t => !string.IsNullOrWhiteSpace(t.Symbol)
                && t.Symbol!.EndsWith("_USDT", StringComparison.OrdinalIgnoreCase))
            .ToList();

        // Build lookup dictionaries keyed by uppercase ticker symbol.
        var projectLookup = projectsRaw
            .GroupBy(p => p.Ticker!.ToUpperInvariant(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var p = g.First();
                    return new ProjectScoreInfo(p.BrokerScore, p.SecurityScore, p.TwitterScore,
                        p.Rank, p.TotalRaiseUsd, p.PercentCirculating, p.MarketCapUsd, p.FdmcUsd);
                },
                StringComparer.OrdinalIgnoreCase);

        var unlockLookup = unlocksRaw
            .GroupBy(u => u.Ticker!.ToUpperInvariant(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g => g.Max(u => u.UnlockPercent),
                StringComparer.OrdinalIgnoreCase);

        // Pre-sort values used for percentile rank computation.
        var sortedAmount24 = validTickers
            .Where(t => t.Amount24.HasValue)
            .Select(t => t.Amount24!.Value)
            .OrderBy(v => v)
            .ToList();

        var sortedHoldVol = validTickers
            .Where(t => t.HoldVol.HasValue)
            .Select(t => t.HoldVol!.Value)
            .OrderBy(v => v)
            .ToList();

        // Twitter score percentile is computed only among MEXC-matched projects.
        var mexcBaseTickers = validTickers
            .Select(t => t.Symbol!.Split('_')[0].ToUpperInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var sortedTwitter = projectLookup
            .Where(kv => mexcBaseTickers.Contains(kv.Key) && kv.Value.TwitterScore.HasValue)
            .Select(kv => (decimal)kv.Value.TwitterScore!.Value)
            .OrderBy(v => v)
            .ToList();

        var mappedData = validTickers
            .Select(ticker =>
            {
                var baseTicker = ticker.Symbol!.Split('_')[0].ToUpperInvariant();
                projectLookup.TryGetValue(baseTicker, out var project);
                unlockLookup.TryGetValue(baseTicker, out var unlockPercent);
                var breakdown = ComputeBreakdown(ticker, sortedAmount24, sortedHoldVol, sortedTwitter, project, unlockPercent);

                return new MexcContractTickerDto
                {
                    ContractId = ticker.ContractId,
                    Symbol = ticker.Symbol!,
                    LastPrice = ticker.LastPrice,
                    Bid1 = ticker.Bid1,
                    Ask1 = ticker.Ask1,
                    High24Price = ticker.High24Price,
                    Low24Price = ticker.Low24Price,
                    Volume24 = ticker.Volume24,
                    Amount24 = ticker.Amount24,
                    HoldVol = ticker.HoldVol,
                    RiseFallRate = ticker.RiseFallRate,
                    RiseFallValue = ticker.RiseFallValue,
                    IndexPrice = ticker.IndexPrice,
                    FairPrice = ticker.FairPrice,
                    FundingRate = ticker.FundingRate,
                    MaxBidPrice = ticker.MaxBidPrice,
                    MinAskPrice = ticker.MinAskPrice,
                    RiseFallRates = ticker.RiseFallRates,
                    RiseFallRatesOfTimezone = ticker.RiseFallRatesOfTimezone,
                    Timestamp = ticker.Timestamp,
                    CompositeScore = ComputeComposite(breakdown),
                    ScoreBreakdown = breakdown
                };
            })
            .OrderByDescending(t => t.CompositeScore)
            .ToList();

        return new MexcContractTickersResponse
        {
            Data = mappedData,
            Success = true,
            Message = "Success"
        };
    }

    private static MexcScoreBreakdownDto ComputeBreakdown(
        InternalMexcTicker ticker,
        IReadOnlyList<decimal> sortedAmount24,
        IReadOnlyList<decimal> sortedHoldVol,
        IReadOnlyList<decimal> sortedTwitter,
        ProjectScoreInfo? project,
        decimal? unlockPercent)
    {
        // VolumeScore (0-25): percentile rank of 24h USD volume.
        var volumeScore = ticker.Amount24.HasValue
            ? Math.Round(ComputePercentileRank(sortedAmount24, ticker.Amount24.Value) * 25m, 2)
            : 0m;

        // OpenInterestScore (0-20): percentile rank of open interest.
        var oiScore = ticker.HoldVol.HasValue
            ? Math.Round(ComputePercentileRank(sortedHoldVol, ticker.HoldVol.Value) * 20m, 2)
            : 0m;

        // FundingScore (0-15): closest to 0 funding rate scores highest.
        // Multiplier 10000 differentiates normal (0.00003) from extreme (0.002) funding rates.
        var fundingScore = ticker.FundingRate.HasValue
            ? Math.Round(Math.Max(0m, 15m - Math.Min(Math.Abs(ticker.FundingRate.Value) * 10000m, 15m)), 2)
            : 7.5m;

        // MomentumScore (0-10): 24h rise/fall rate clamped to [-5%, +5%] remapped to [0, 10].
        var rate = ticker.RiseFallRate ?? 0m;
        var clampedRate = Math.Max(-0.05m, Math.Min(0.05m, rate));
        var momentumScore = Math.Round((clampedRate + 0.05m) / 0.1m * 10m, 2);

        // BrokerQualityScore (0-15): blend of BrokerScore (0-10), SecurityScore (0-100), Rank tier.
        var brokerPartial = project?.BrokerScore is { } bs ? (bs / 10m) * 8m : 0m;
        var securityPartial = project?.SecurityScore is { } ss ? (ss / 100m) * 5m : 0m;
        var rankBonus = project?.Rank switch
        {
            <= 100 => 2m,
            <= 500 => 1m,
            _ => 0m
        };
        var brokerQualityScore = Math.Round(Math.Min(brokerPartial + securityPartial + rankBonus, 15m), 2);

        // SocialScore (0-5): Twitter engagement percentile among MEXC-matched projects.
        var socialScore = project?.TwitterScore is { } ts && sortedTwitter.Count > 0
            ? Math.Round(ComputePercentileRank(sortedTwitter, ts) * 5m, 2)
            : 0m;

        // FundraisingScore (0-5): log-normalized total raise (cap $50M = full 5pts).
        var fundraisingScore = 0m;
        if (project?.TotalRaiseUsd is { } raise && raise > 0)
        {
            var logVal = (decimal)Math.Log10((double)(raise + 1));
            var logMax = (decimal)Math.Log10(50_000_001.0);
            fundraisingScore = Math.Round(Math.Min(logVal / logMax * 5m, 5m), 2);
        }

        // SupplyHealthScore (0-5): higher circulating supply = less future sell pressure.
        var supplyScore = project?.PercentCirculating switch
        {
            >= 70 => 5m,
            >= 50 => 3m,
            >= 30 => 1m,
            _ => 0m
        };

        // UnlockPenalty (0 to -15): imminent token unlock within 14 days.
        var unlockPenalty = unlockPercent switch
        {
            >= 10 => -15m,
            >= 5 => -10m,
            >= 2 => -5m,
            > 0 => -2m,
            _ => 0m
        };

        // FdvOverhangPenalty (0 to -5): FDV >> market cap signals future dilution risk.
        var fdvPenalty = 0m;
        if (project is { MarketCapUsd: > 0 } && project.FdmcUsd is { } fdmc && fdmc > 0)
        {
            var ratio = fdmc / project.MarketCapUsd!.Value;
            fdvPenalty = ratio switch
            {
                > 5m => -5m,
                > 3m => -3m,
                _ => 0m
            };
        }

        return new MexcScoreBreakdownDto
        {
            VolumeScore = volumeScore,
            OpenInterestScore = oiScore,
            FundingScore = fundingScore,
            MomentumScore = momentumScore,
            BrokerQualityScore = brokerQualityScore,
            SocialScore = socialScore,
            FundraisingScore = fundraisingScore,
            SupplyHealthScore = supplyScore,
            UnlockPenalty = unlockPenalty,
            FdvOverhangPenalty = fdvPenalty
        };
    }

    private static decimal ComputeComposite(MexcScoreBreakdownDto b) =>
        Math.Round(Math.Clamp(
            b.VolumeScore + b.OpenInterestScore + b.FundingScore + b.MomentumScore +
            b.BrokerQualityScore + b.SocialScore + b.FundraisingScore + b.SupplyHealthScore +
            b.UnlockPenalty + b.FdvOverhangPenalty,
            0m, 100m), 2);

    /// <summary>
    /// Computes the percentile rank of <paramref name="value"/> in a pre-sorted ascending list.
    /// Returns the fraction of values strictly less than <paramref name="value"/>.
    /// </summary>
    private static decimal ComputePercentileRank(IReadOnlyList<decimal> sortedAscending, decimal value)
    {
        if (sortedAscending.Count == 0) return 0m;
        var lo = 0;
        var hi = sortedAscending.Count;
        while (lo < hi)
        {
            var mid = lo + (hi - lo) / 2;
            if (sortedAscending[mid] < value) lo = mid + 1;
            else hi = mid;
        }
        return (decimal)lo / sortedAscending.Count;
    }

    private sealed record ProjectScoreInfo(
        decimal? BrokerScore,
        decimal? SecurityScore,
        int? TwitterScore,
        int? Rank,
        decimal? TotalRaiseUsd,
        decimal? PercentCirculating,
        decimal? MarketCapUsd,
        decimal? FdmcUsd);

    private sealed record InternalMexcApiArrayResponse
    {
        public bool Success { get; init; }
        public int Code { get; init; }
        public List<InternalMexcTicker>? Data { get; init; }
    }

    private sealed record InternalMexcTicker
    {
        public int? ContractId { get; init; }
        public string? Symbol { get; init; }
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
    }
}
