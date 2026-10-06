using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExternalData.MarketData.Queries.GetMexcSpotTicker24Hr;

/// <summary>
/// Handler for GetMexcSpotTicker24HrQuery.
/// Retrieves MEXC spot 24h ticker statistics enriched with composite scoring
/// using ChainBroker project quality and unlock data.
/// </summary>
public sealed class GetMexcSpotTicker24HrQueryHandler
    : IRequestHandler<GetMexcSpotTicker24HrQuery, MexcSpotTicker24HrResponse>
{
    private readonly IExternalDataService _externalDataService;
    private readonly IApplicationDbContext _dbContext;

    // Common quote currency suffixes in priority order (longest first to avoid partial matches).
    private static readonly string[] QuoteSuffixes = ["USDT", "USDC", "BUSD", "BTC", "ETH", "BNB", "TRX", "USD"];

    public GetMexcSpotTicker24HrQueryHandler(
        IExternalDataService externalDataService,
        IApplicationDbContext dbContext)
    {
        _externalDataService = externalDataService;
        _dbContext = dbContext;
    }

    public async Task<MexcSpotTicker24HrResponse> Handle(
        GetMexcSpotTicker24HrQuery request,
        CancellationToken cancellationToken)
    {
        // Kick off the API call immediately so it runs concurrently with DB queries.
        var apiTask = _externalDataService
            .GetMexcSpotTicker24HrAsync<List<InternalMexcSpotTicker24Hr>>(cancellationToken);

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
                p.FdmcUsd,
                p.PrivateRoi,
                p.PublicRoi,
                p.ListingDate,
                FundCount = p.Funds.Count()
            })
            .ToListAsync(cancellationToken);

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var cutoff = today.AddDays(14);

        var unlocksRaw = await _dbContext.ChainBrokerUnlocks
            .Where(u => u.Ticker != null
                     && u.NextUnlockDate.HasValue
                     && u.NextUnlockDate >= today
                     && u.NextUnlockDate <= cutoff)
            .Select(u => new { u.Ticker, u.UnlockPercent, u.RoundName })
            .ToListAsync(cancellationToken);

        var apiResponse = await apiTask;

        if (apiResponse is null)
        {
            return new MexcSpotTicker24HrResponse
            {
                Data = [],
                Success = false,
                Message = "Failed to retrieve MEXC spot 24h ticker data from external API."
            };
        }

        var validTickers = apiResponse
            .Where(x => !string.IsNullOrWhiteSpace(x.Symbol)
                && x.Symbol!.EndsWith("USDT", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var projectLookup = projectsRaw
            .GroupBy(p => p.Ticker!.ToUpperInvariant(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var p = g.First();
                    return new ProjectScoreInfo(
                        p.BrokerScore, p.SecurityScore, p.TwitterScore,
                        p.Rank, p.TotalRaiseUsd, p.PercentCirculating,
                        p.MarketCapUsd, p.FdmcUsd, p.PrivateRoi,
                        p.PublicRoi, p.ListingDate, p.FundCount);
                },
                StringComparer.OrdinalIgnoreCase);

        var unlockLookup = unlocksRaw
            .GroupBy(u => u.Ticker!.ToUpperInvariant(), StringComparer.OrdinalIgnoreCase)
            .ToDictionary(
                g => g.Key,
                g =>
                {
                    var maxPercent = g.Max(u => u.UnlockPercent);
                    var hasDangerousRound = g.Any(u => u.RoundName != null &&
                        (u.RoundName.Contains("Team", StringComparison.OrdinalIgnoreCase) ||
                         u.RoundName.Contains("Seed", StringComparison.OrdinalIgnoreCase) ||
                         u.RoundName.Contains("Investor", StringComparison.OrdinalIgnoreCase) ||
                         u.RoundName.Contains("Private", StringComparison.OrdinalIgnoreCase)));
                    return (Percent: maxPercent, HasDangerousRound: hasDangerousRound);
                },
                StringComparer.OrdinalIgnoreCase);

        // Pre-sort QuoteVolume values for percentile computation.
        var sortedQuoteVolume = validTickers
            .Select(x => ParseDecimal(x.QuoteVolume))
            .Where(v => v.HasValue)
            .Select(v => v!.Value)
            .OrderBy(v => v)
            .ToList();

        // Twitter score percentile among MEXC-matched projects only.
        var mexcBaseTickers = validTickers
            .Select(x => ExtractBaseTicker(x.Symbol!).ToUpperInvariant())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var sortedTwitter = projectLookup
            .Where(kv => mexcBaseTickers.Contains(kv.Key) && kv.Value.TwitterScore.HasValue)
            .Select(kv => (decimal)kv.Value.TwitterScore!.Value)
            .OrderBy(v => v)
            .ToList();

        var mappedData = validTickers
            .Select(x =>
            {
                var baseTicker = ExtractBaseTicker(x.Symbol!).ToUpperInvariant();
                projectLookup.TryGetValue(baseTicker, out var project);
                unlockLookup.TryGetValue(baseTicker, out var unlockData);

                var quoteVol = ParseDecimal(x.QuoteVolume);
                var pricePct = ParseDecimal(x.PriceChangePercent);

                var breakdown = ComputeBreakdown(
                    quoteVol, pricePct, sortedQuoteVolume, sortedTwitter,
                    project, unlockData.Percent, unlockData.HasDangerousRound, today);

                return new MexcSpotTicker24HrDto
                {
                    Symbol = x.Symbol!,
                    PriceChange = x.PriceChange,
                    PriceChangePercent = x.PriceChangePercent,
                    PrevClosePrice = x.PrevClosePrice,
                    LastPrice = x.LastPrice,
                    BidPrice = x.BidPrice,
                    BidQty = x.BidQty,
                    AskPrice = x.AskPrice,
                    AskQty = x.AskQty,
                    OpenPrice = x.OpenPrice,
                    HighPrice = x.HighPrice,
                    LowPrice = x.LowPrice,
                    Volume = x.Volume,
                    QuoteVolume = x.QuoteVolume,
                    OpenTime = x.OpenTime,
                    CloseTime = x.CloseTime,
                    Count = x.Count,
                    CompositeScore = ComputeComposite(breakdown),
                    ScoreBreakdown = breakdown
                };
            })
            .OrderByDescending(x => x.CompositeScore)
            .ToList();

        return new MexcSpotTicker24HrResponse
        {
            Data = mappedData,
            Success = true,
            Message = "Success"
        };
    }

    private static MexcSpotScoreBreakdownDto ComputeBreakdown(
        decimal? quoteVolume,
        decimal? priceChangePercent,
        IReadOnlyList<decimal> sortedQuoteVolume,
        IReadOnlyList<decimal> sortedTwitter,
        ProjectScoreInfo? project,
        decimal? unlockPercent,
        bool unlockHasDangerousRound,
        DateOnly today)
    {
        // VolumeScore (0-35): percentile rank of 24h USD quote volume.
        var volumeScore = quoteVolume.HasValue
            ? Math.Round(ComputePercentileRank(sortedQuoteVolume, quoteVolume.Value) * 35m, 2)
            : 0m;

        // MomentumScore (0-15): 24h price change rate clamped to [-5%, +5%] remapped to [0, 15].
        var rate = priceChangePercent ?? 0m;
        var clampedRate = Math.Max(-0.05m, Math.Min(0.05m, rate));
        var momentumScore = Math.Round((clampedRate + 0.05m) / 0.1m * 15m, 2);

        // BrokerQualityScore (0-20): blend of BrokerScore, SecurityScore, and Rank tier.
        var brokerPartial = project?.BrokerScore is { } bs ? (bs / 10m) * 11m : 0m;
        var securityPartial = project?.SecurityScore is { } ss ? (ss / 100m) * 7m : 0m;
        var rankBonus = project?.Rank switch
        {
            <= 100 => 2m,
            <= 500 => 1m,
            _ => 0m
        };
        var brokerQualityScore = Math.Round(Math.Min(brokerPartial + securityPartial + rankBonus, 20m), 2);

        // SocialScore (0-10): Twitter engagement percentile among MEXC-matched projects.
        var socialScore = project?.TwitterScore is { } ts && sortedTwitter.Count > 0
            ? Math.Round(ComputePercentileRank(sortedTwitter, ts) * 10m, 2)
            : 0m;

        // FundraisingScore (0-10): log-normalised total raise (cap $50M = 10pts).
        var fundraisingScore = 0m;
        if (project?.TotalRaiseUsd is { } raise && raise > 0)
        {
            var logVal = (decimal)Math.Log10((double)(raise + 1));
            var logMax = (decimal)Math.Log10(50_000_001.0);
            fundraisingScore = Math.Round(Math.Min(logVal / logMax * 10m, 10m), 2);
        }

        // SupplyHealthScore (0-10): higher circulating % = less future sell pressure.
        var supplyScore = project?.PercentCirculating switch
        {
            >= 70 => 10m,
            >= 50 => 6m,
            >= 30 => 2m,
            _ => 0m
        };

        // UnlockPenalty (0 to -15): imminent token unlock within 14 days.
        // Extra -3 when the vesting round is Team/Seed/Investor/Private (higher sell intent), capped at -15.
        var basePenalty = unlockPercent switch
        {
            >= 10 => -15m,
            >= 5 => -10m,
            >= 2 => -5m,
            > 0 => -2m,
            _ => 0m
        };
        var unlockPenalty = unlockHasDangerousRound && basePenalty < 0
            ? Math.Max(basePenalty - 3m, -15m)
            : basePenalty;

        // FdvOverhangPenalty (0 to -5): FDV >> market cap signals future dilution.
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

        // PrivateSaleOverhangPenalty (0 to -10): private investors sitting on large gains = sell pressure.
        // PrivateRoi = currentPrice / privateSalePrice; e.g. 3.0 means current is 3x private price.
        var privateSalePenalty = project?.PrivateRoi switch
        {
            > 10m => -10m,
            > 5m => -7m,
            > 3m => -4m,
            > 1m => -1m,
            _ => 0m
        };

        // PublicSaleOverhangPenalty (0 to -5): IDO/IEO investors in profit = sell pressure.
        // Smaller range than private because public allocation sizes are typically smaller.
        var publicSalePenalty = project?.PublicRoi switch
        {
            > 5m => -5m,
            > 3m => -3m,
            > 1m => -1m,
            _ => 0m
        };

        // NewListingPenalty (0 to -3): recently listed token has higher early-vesting dump risk.
        var newListingPenalty = 0m;
        if (project?.ListingDate is { } listingDate)
        {
            var daysSinceListing = today.DayNumber - listingDate.DayNumber;
            newListingPenalty = daysSinceListing switch
            {
                < 90 => -3m,
                < 180 => -1m,
                _ => 0m
            };
        }

        // VcBackingBonus (0 to +5): more VC backers = stronger due-diligence signal.
        var vcBonus = project?.FundCount switch
        {
            >= 3 => 5m,
            2 => 3m,
            1 => 1m,
            _ => 0m
        };

        return new MexcSpotScoreBreakdownDto
        {
            VolumeScore = volumeScore,
            MomentumScore = momentumScore,
            BrokerQualityScore = brokerQualityScore,
            SocialScore = socialScore,
            FundraisingScore = fundraisingScore,
            SupplyHealthScore = supplyScore,
            UnlockPenalty = unlockPenalty,
            FdvOverhangPenalty = fdvPenalty,
            PrivateSaleOverhangPenalty = privateSalePenalty,
            PublicSaleOverhangPenalty = publicSalePenalty,
            NewListingPenalty = newListingPenalty,
            VcBackingBonus = vcBonus
        };
    }

    private static decimal ComputeComposite(MexcSpotScoreBreakdownDto b) =>
        Math.Round(Math.Clamp(
            b.VolumeScore + b.MomentumScore + b.BrokerQualityScore +
            b.SocialScore + b.FundraisingScore + b.SupplyHealthScore +
            b.UnlockPenalty + b.FdvOverhangPenalty + b.PrivateSaleOverhangPenalty +
            b.PublicSaleOverhangPenalty + b.NewListingPenalty + b.VcBackingBonus,
            0m, 100m), 2);

    /// <summary>
    /// Binary-search percentile: fraction of values in <paramref name="sortedAscending"/> strictly less than <paramref name="value"/>.
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

    /// <summary>
    /// Strips the quote currency suffix from a MEXC spot symbol to extract the base ticker.
    /// E.g. "BTCUSDT" -> "BTC", "NMRETH" -> "NMR".
    /// </summary>
    private static string ExtractBaseTicker(string symbol)
    {
        foreach (var suffix in QuoteSuffixes)
        {
            if (symbol.EndsWith(suffix, StringComparison.OrdinalIgnoreCase) && symbol.Length > suffix.Length)
                return symbol[..^suffix.Length];
        }
        return symbol;
    }

    private static decimal? ParseDecimal(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        return decimal.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
            ? result
            : null;
    }

    private sealed record ProjectScoreInfo(
        decimal? BrokerScore,
        decimal? SecurityScore,
        int? TwitterScore,
        int? Rank,
        decimal? TotalRaiseUsd,
        decimal? PercentCirculating,
        decimal? MarketCapUsd,
        decimal? FdmcUsd,
        decimal? PrivateRoi,
        decimal? PublicRoi,
        DateOnly? ListingDate,
        int FundCount);

    private sealed record InternalMexcSpotTicker24Hr
    {
        public string? Symbol { get; init; }
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
    }
}
