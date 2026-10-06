using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.ChainBroker;

/// <summary>
/// Snapshot of a crypto project from the ChainBroker API.
/// All monetary values are stored in raw USD; percentages as plain decimals.
/// </summary>
public sealed class ChainBrokerProject : BaseEntity
{
    private readonly List<ChainBrokerProjectBlockchain> _blockchains = [];
    private readonly List<ChainBrokerProjectTag> _tags = [];
    private readonly List<ChainBrokerProjectFundRef> _funds = [];
    private readonly List<ChainBrokerProjectLaunchpadRef> _launchpads = [];

    private ChainBrokerProject() { }

    /// <summary>Natural key from ChainBroker API.</summary>
    public string Slug { get; private set; } = string.Empty;
    public string? Name { get; private set; }
    public string? Logo { get; private set; }
    public string? Ticker { get; private set; }

    // --- Prices (USD) ---
    public decimal? CurrentPriceUsd { get; private set; }
    public decimal? PublicPriceUsd { get; private set; }
    public decimal? PrivatePriceUsd { get; private set; }
    public decimal? AthPriceUsd { get; private set; }
    public decimal? AtlPriceUsd { get; private set; }

    // --- ROI multiples (e.g. 3.28 for "3.28x") ---
    public decimal? PublicRoi { get; private set; }
    public decimal? PrivateRoi { get; private set; }
    public decimal? PublicAthRoi { get; private set; }
    public decimal? PrivateAthRoi { get; private set; }

    // --- Scores ---
    /// <summary>ChainBroker composite score (e.g. 7.51).</summary>
    public decimal? BrokerScore { get; private set; }
    /// <summary>Security audit score (e.g. 77.91).</summary>
    public decimal? SecurityScore { get; private set; }
    /// <summary>Twitter / X engagement score (integer, nullable).</summary>
    public int? TwitterScore { get; private set; }
    public int? Rank { get; private set; }

    // --- Fundraising (USD) ---
    public decimal? PublicRaiseUsd { get; private set; }
    public decimal? PrivateRaiseUsd { get; private set; }
    public decimal? TotalRaiseUsd { get; private set; }

    // --- Key dates ---
    public DateOnly? PrivateAnnounceDate { get; private set; }
    public DateOnly? ListingDate { get; private set; }
    public DateOnly? IdoDate { get; private set; }
    public DateOnly? NextUnlockDate { get; private set; }

    // --- Market data (USD) ---
    public decimal? MarketCapUsd { get; private set; }
    public decimal? ReportedMarketCapUsd { get; private set; }
    public decimal? InitialMarketCapUsd { get; private set; }
    public decimal? FdmcUsd { get; private set; }
    public decimal? InitialFdmcUsd { get; private set; }
    public decimal? Volume24hUsd { get; private set; }

    // --- Token supply (raw token count) ---
    public decimal? CurrentCirculation { get; private set; }
    public decimal? InitialCirculation { get; private set; }
    public decimal? TotalCirculation { get; private set; }
    /// <summary>Percentage of total supply currently circulating (e.g. 20.3 for "20.3%").</summary>
    public decimal? PercentCirculating { get; private set; }

    // --- Price changes (%) ---
    public decimal? PriceChange24h { get; private set; }
    public decimal? PriceChange7d { get; private set; }
    public decimal? PriceChange30d { get; private set; }
    public decimal? PriceChange1y { get; private set; }

    // --- Related entities ---
    public IReadOnlyCollection<ChainBrokerProjectBlockchain> Blockchains => _blockchains.AsReadOnly();
    public IReadOnlyCollection<ChainBrokerProjectTag> Tags => _tags.AsReadOnly();
    public IReadOnlyCollection<ChainBrokerProjectFundRef> Funds => _funds.AsReadOnly();
    public IReadOnlyCollection<ChainBrokerProjectLaunchpadRef> Launchpads => _launchpads.AsReadOnly();

    public static ChainBrokerProject Create(ChainBrokerProjectSnapshot snapshot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshot.Slug);
        var entity = new ChainBrokerProject { Id = Guid.NewGuid() };
        ApplyScalars(entity, snapshot);
        entity.UpdateBlockchains(snapshot.Blockchains);
        entity.UpdateTags(snapshot.Tags);
        entity.UpdateFunds(snapshot.Funds);
        entity.UpdateLaunchpads(snapshot.Launchpads);
        return entity;
    }

    public void Update(ChainBrokerProjectSnapshot snapshot)
    {
        ApplyScalars(this, snapshot);
        UpdateBlockchains(snapshot.Blockchains);
        UpdateTags(snapshot.Tags);
        UpdateFunds(snapshot.Funds);
        UpdateLaunchpads(snapshot.Launchpads);
    }

    public void UpdateWithoutFunds(ChainBrokerProjectSnapshot snapshot)
    {
        ApplyScalars(this, snapshot);
        UpdateBlockchains(snapshot.Blockchains);
        UpdateTags(snapshot.Tags);
        UpdateLaunchpads(snapshot.Launchpads);
    }

    public void UpdateBlockchains(IReadOnlyList<string> incoming)
    {
        var incomingSet = new HashSet<string>(incoming, StringComparer.OrdinalIgnoreCase);

        for (int i = _blockchains.Count - 1; i >= 0; i--)
        {
            if (!incomingSet.Contains(_blockchains[i].Name))
                _blockchains.RemoveAt(i);
        }

        var existingSet = new HashSet<string>(_blockchains.Select(b => b.Name), StringComparer.OrdinalIgnoreCase);
        foreach (var name in incoming)
        {
            if (existingSet.Add(name))
                _blockchains.Add(ChainBrokerProjectBlockchain.Create(Id, name));
        }
    }

    public void UpdateTags(IReadOnlyList<(string Name, string? Slug)> incoming)
    {
        var incomingNames = new HashSet<string>(incoming.Select(t => t.Name), StringComparer.OrdinalIgnoreCase);

        for (int i = _tags.Count - 1; i >= 0; i--)
        {
            if (!incomingNames.Contains(_tags[i].Name))
                _tags.RemoveAt(i);
        }

        var existingNames = new HashSet<string>(_tags.Select(t => t.Name), StringComparer.OrdinalIgnoreCase);
        foreach (var (name, slug) in incoming)
        {
            if (existingNames.Add(name))
                _tags.Add(ChainBrokerProjectTag.Create(Id, name, slug));
        }
    }

    public void UpdateFunds(IReadOnlyList<Guid> incomingFundIds)
    {
        var incomingSet = new HashSet<Guid>(incomingFundIds);

        for (int i = _funds.Count - 1; i >= 0; i--)
        {
            if (!incomingSet.Contains(_funds[i].FundId))
                _funds.RemoveAt(i);
        }

        var existingIds = new HashSet<Guid>(_funds.Select(f => f.FundId));
        foreach (var fundId in incomingFundIds)
        {
            if (existingIds.Add(fundId))
                _funds.Add(ChainBrokerProjectFundRef.Create(Id, fundId));
        }
    }

    public void UpdateLaunchpads(IReadOnlyList<(string Slug, string? Name)> incoming)
    {
        var incomingSlugs = new HashSet<string>(incoming.Select(l => l.Slug), StringComparer.OrdinalIgnoreCase);

        for (int i = _launchpads.Count - 1; i >= 0; i--)
        {
            if (!incomingSlugs.Contains(_launchpads[i].Slug))
                _launchpads.RemoveAt(i);
        }

        var existingSlugs = new HashSet<string>(_launchpads.Select(l => l.Slug), StringComparer.OrdinalIgnoreCase);
        foreach (var (slug, name) in incoming)
        {
            if (existingSlugs.Add(slug))
                _launchpads.Add(ChainBrokerProjectLaunchpadRef.Create(Id, slug, name));
        }
    }

    private static void ApplyScalars(ChainBrokerProject e, ChainBrokerProjectSnapshot s)
    {
        e.Slug = s.Slug.Trim();
        e.Name = s.Name;
        e.Logo = s.Logo;
        e.Ticker = s.Ticker;

        e.CurrentPriceUsd = ChainBrokerValueParser.ParseUsd(s.CurrentPrice);
        e.PublicPriceUsd = ChainBrokerValueParser.ParseUsd(s.PublicPrice);
        e.PrivatePriceUsd = ChainBrokerValueParser.ParseUsd(s.PrivatePrice);
        e.AthPriceUsd = ChainBrokerValueParser.ParseUsd(s.AthPrice);
        e.AtlPriceUsd = ChainBrokerValueParser.ParseUsd(s.AtlPrice);

        e.PublicRoi = ChainBrokerValueParser.ParseRoi(s.PublicRoi);
        e.PrivateRoi = ChainBrokerValueParser.ParseRoi(s.PrivateRoi);
        e.PublicAthRoi = ChainBrokerValueParser.ParseRoi(s.PublicAthRoi);
        e.PrivateAthRoi = ChainBrokerValueParser.ParseRoi(s.PrivateAthRoi);

        e.BrokerScore = ChainBrokerValueParser.ParseDecimal(s.BrokerScore);
        e.SecurityScore = ChainBrokerValueParser.ParseDecimal(s.SecurityScore);
        e.TwitterScore = s.TwitterScore;
        e.Rank = s.Rank;

        e.PublicRaiseUsd = ChainBrokerValueParser.ParseUsd(s.PublicRaise);
        e.PrivateRaiseUsd = ChainBrokerValueParser.ParseUsd(s.PrivateRaise);
        e.TotalRaiseUsd = ChainBrokerValueParser.ParseUsd(s.TotalRaise);

        e.PrivateAnnounceDate = ChainBrokerValueParser.ParseDate(s.PrivateAnnounceDate);
        e.ListingDate = ChainBrokerValueParser.ParseDate(s.ListingDate);
        e.IdoDate = ChainBrokerValueParser.ParseDate(s.IdoDate);
        e.NextUnlockDate = ChainBrokerValueParser.ParseDate(s.NextUnlock);

        e.MarketCapUsd = ChainBrokerValueParser.ParseUsd(s.MarketCap);
        e.ReportedMarketCapUsd = ChainBrokerValueParser.ParseUsd(s.ReportedMarketCap);
        e.InitialMarketCapUsd = ChainBrokerValueParser.ParseUsd(s.InitialMarketCap);
        e.FdmcUsd = ChainBrokerValueParser.ParseUsd(s.Fdmc);
        e.InitialFdmcUsd = ChainBrokerValueParser.ParseUsd(s.InitialFdmc);
        e.Volume24hUsd = ChainBrokerValueParser.ParseUsd(s.Volume24h);

        e.CurrentCirculation = ChainBrokerValueParser.ParseCount(s.CurrentCirculation);
        e.InitialCirculation = ChainBrokerValueParser.ParseCount(s.InitialCirculation);
        e.TotalCirculation = ChainBrokerValueParser.ParseCount(s.TotalCirculation);
        e.PercentCirculating = ChainBrokerValueParser.ParsePercent(s.PercentCirculating);

        e.PriceChange24h = ChainBrokerValueParser.ParsePercent(s.PriceChange24h);
        e.PriceChange7d = ChainBrokerValueParser.ParsePercent(s.PriceChange7d);
        e.PriceChange30d = ChainBrokerValueParser.ParsePercent(s.PriceChange30d);
        e.PriceChange1y = ChainBrokerValueParser.ParsePercent(s.PriceChange1y);
    }
}

/// <summary>
/// Raw values from the ChainBroker projects API.
/// Passed to <see cref="ChainBrokerProject.Create"/> or <see cref="ChainBrokerProject.Update"/>.
/// </summary>
public sealed record ChainBrokerProjectSnapshot
{
    public required string Slug { get; init; }
    public string? Name { get; init; }
    public string? Logo { get; init; }
    public string? Ticker { get; init; }

    // Raw price strings
    public string? CurrentPrice { get; init; }
    public string? PublicPrice { get; init; }
    public string? PrivatePrice { get; init; }
    public string? AthPrice { get; init; }
    public string? AtlPrice { get; init; }

    // Raw ROI strings
    public string? PublicRoi { get; init; }
    public string? PrivateRoi { get; init; }
    public string? PublicAthRoi { get; init; }
    public string? PrivateAthRoi { get; init; }

    // Scores (broker_score and security_score are strings in API; rank and twitter_score are integers)
    public string? BrokerScore { get; init; }
    public string? SecurityScore { get; init; }
    public int? TwitterScore { get; init; }
    public int? Rank { get; init; }

    // Raw raise strings
    public string? PublicRaise { get; init; }
    public string? PrivateRaise { get; init; }
    public string? TotalRaise { get; init; }

    // Dates
    public string? PrivateAnnounceDate { get; init; }
    public string? ListingDate { get; init; }
    public string? IdoDate { get; init; }
    public string? NextUnlock { get; init; }

    // Raw market data strings
    public string? MarketCap { get; init; }
    public string? ReportedMarketCap { get; init; }
    public string? InitialMarketCap { get; init; }
    public string? Fdmc { get; init; }
    public string? InitialFdmc { get; init; }
    public string? Volume24h { get; init; }

    // Raw circulation strings
    public string? CurrentCirculation { get; init; }
    public string? InitialCirculation { get; init; }
    public string? TotalCirculation { get; init; }
    public string? PercentCirculating { get; init; }

    // Raw price change strings
    public string? PriceChange24h { get; init; }
    public string? PriceChange7d { get; init; }
    public string? PriceChange30d { get; init; }
    public string? PriceChange1y { get; init; }

    // Related entities: (name) for blockchains, (name, slug) for others
    public IReadOnlyList<string> Blockchains { get; init; } = [];
    public IReadOnlyList<(string Name, string? Slug)> Tags { get; init; } = [];
    public IReadOnlyList<Guid> Funds { get; init; } = [];
    public IReadOnlyList<(string Slug, string? Name)> Launchpads { get; init; } = [];
}
