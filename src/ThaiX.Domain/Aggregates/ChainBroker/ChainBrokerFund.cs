using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.ChainBroker;

/// <summary>
/// Snapshot of a VC fund from the ChainBroker API.
/// All monetary averages are stored in raw USD; percentages as plain decimals.
/// </summary>
public sealed class ChainBrokerFund : BaseEntity
{
    private ChainBrokerFund() { }

    /// <summary>Natural key from ChainBroker API (e.g. "slow-ventures").</summary>
    public string Slug { get; private set; } = string.Empty;
    public string? Name { get; private set; }
    public string? Logo { get; private set; }

    /// <summary>Fund category name (e.g. "Crypto Ventures", "Non-Crypto Capital").</summary>
    public string? FundTypeName { get; private set; }
    public string? FundTypeSlug { get; private set; }

    public DateOnly? LastInvestmentDate { get; private set; }
    public int? YearFounded { get; private set; }

    /// <summary>Fund activity status from ChainBroker (e.g. "Active", "Passive").</summary>
    public string? Status { get; private set; }

    // --- Portfolio averages ---

    /// <summary>Average current ROI multiple across all portfolio projects (e.g. 16.2 for "16.2x").</summary>
    public decimal? AverageCurrentRoi { get; private set; }
    public decimal? AverageMarketCapUsd { get; private set; }
    public decimal? AverageInitialMarketCapUsd { get; private set; }
    public decimal? AverageFdmcUsd { get; private set; }
    public decimal? AverageInitialFdmcUsd { get; private set; }
    public decimal? AveragePublicRaiseUsd { get; private set; }
    public decimal? AveragePrivateRaiseUsd { get; private set; }
    public decimal? AverageTotalRaiseUsd { get; private set; }

    public decimal? AveragePriceChange24h { get; private set; }
    public decimal? AveragePriceChange7d { get; private set; }
    public decimal? AveragePriceChange30d { get; private set; }
    public decimal? AveragePriceChange1y { get; private set; }

    /// <summary>Total number of known portfolio investments.</summary>
    public int ProjectCount { get; private set; }

    // --- Gainers/Losers (from gainers_losers array in API: [count, "percent%"]) ---

    /// <summary>Number of portfolio projects currently trading above their initial price.</summary>
    public int? GainersCount { get; private set; }
    /// <summary>Gainers as a percentage of tracked projects (e.g. 40.0 for "40%").</summary>
    public decimal? GainersPercent { get; private set; }
    /// <summary>Number of portfolio projects currently trading below their initial price.</summary>
    public int? LosersCount { get; private set; }
    /// <summary>Losers as a percentage of tracked projects (e.g. 60.0 for "60%").</summary>
    public decimal? LosersPercent { get; private set; }

    public static ChainBrokerFund Create(ChainBrokerFundSnapshot snapshot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshot.Slug);
        var entity = new ChainBrokerFund { Id = Guid.NewGuid() };
        ApplyScalars(entity, snapshot);
        return entity;
    }

    public void Update(ChainBrokerFundSnapshot snapshot) => ApplyScalars(this, snapshot);

    private static void ApplyScalars(ChainBrokerFund e, ChainBrokerFundSnapshot s)
    {
        e.Slug = s.Slug.Trim();
        e.Name = s.Name;
        e.Logo = s.Logo;
        e.FundTypeName = s.FundTypeName;
        e.FundTypeSlug = s.FundTypeSlug;
        e.LastInvestmentDate = ChainBrokerValueParser.ParseDate(s.LastInvestment);
        e.YearFounded = s.YearFounded;
        e.Status = s.Status;

        e.AverageCurrentRoi = ChainBrokerValueParser.ParseRoi(s.AverageCurrentRoi);
        e.AverageMarketCapUsd = ChainBrokerValueParser.ParseUsd(s.AverageMarketCap);
        e.AverageInitialMarketCapUsd = ChainBrokerValueParser.ParseUsd(s.AverageInitialMarketCap);
        e.AverageFdmcUsd = ChainBrokerValueParser.ParseUsd(s.AverageFdmc);
        e.AverageInitialFdmcUsd = ChainBrokerValueParser.ParseUsd(s.AverageInitialFdmc);
        e.AveragePublicRaiseUsd = ChainBrokerValueParser.ParseUsd(s.AveragePublicRaise);
        e.AveragePrivateRaiseUsd = ChainBrokerValueParser.ParseUsd(s.AveragePrivateRaise);
        e.AverageTotalRaiseUsd = ChainBrokerValueParser.ParseUsd(s.AverageTotalRaise);

        e.AveragePriceChange24h = ChainBrokerValueParser.ParsePercent(s.AveragePriceChange24h);
        e.AveragePriceChange7d = ChainBrokerValueParser.ParsePercent(s.AveragePriceChange7d);
        e.AveragePriceChange30d = ChainBrokerValueParser.ParsePercent(s.AveragePriceChange30d);
        e.AveragePriceChange1y = ChainBrokerValueParser.ParsePercent(s.AveragePriceChange1y);

        e.ProjectCount = s.ProjectCount;
        e.GainersCount = s.GainersCount;
        e.GainersPercent = ChainBrokerValueParser.ParsePercent(s.GainersPercent);
        e.LosersCount = s.LosersCount;
        e.LosersPercent = ChainBrokerValueParser.ParsePercent(s.LosersPercent);
    }
}

/// <summary>
/// Raw values from the ChainBroker funds API.
/// Passed to <see cref="ChainBrokerFund.Create"/> or <see cref="ChainBrokerFund.Update"/>.
/// </summary>
public sealed record ChainBrokerFundSnapshot
{
    public required string Slug { get; init; }
    public string? Name { get; init; }
    public string? Logo { get; init; }
    public string? FundTypeName { get; init; }
    public string? FundTypeSlug { get; init; }
    public string? LastInvestment { get; init; }

    /// <summary>Already an integer from the API.</summary>
    public int? YearFounded { get; init; }
    public string? Status { get; init; }

    // Raw average strings
    public string? AverageCurrentRoi { get; init; }
    public string? AverageMarketCap { get; init; }
    public string? AverageInitialMarketCap { get; init; }
    public string? AverageFdmc { get; init; }
    public string? AverageInitialFdmc { get; init; }
    public string? AveragePublicRaise { get; init; }
    public string? AveragePrivateRaise { get; init; }
    public string? AverageTotalRaise { get; init; }
    public string? AveragePriceChange24h { get; init; }
    public string? AveragePriceChange7d { get; init; }
    public string? AveragePriceChange30d { get; init; }
    public string? AveragePriceChange1y { get; init; }

    /// <summary>Already an integer from the API.</summary>
    public int ProjectCount { get; init; }

    // gainers_losers from API: [count_int, "percent%"] array — handler extracts these before creating snapshot.
    public int? GainersCount { get; init; }
    public string? GainersPercent { get; init; }
    public int? LosersCount { get; init; }
    public string? LosersPercent { get; init; }

    /// <summary>Sample portfolio projects (slug, name) returned by the API.</summary>
    public IReadOnlyList<(string Slug, string? Name)> Projects { get; init; } = [];
}
