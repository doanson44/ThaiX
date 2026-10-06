using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.ChainBroker;

/// <summary>
/// Snapshot of a token unlock event from the ChainBroker API.
/// Represents the latest fetched state and is overwritten on each sync.
/// </summary>
public sealed class ChainBrokerUnlock : BaseEntity
{
    private ChainBrokerUnlock() { }

    /// <summary>Natural key from ChainBroker API (e.g. "sosovalue").</summary>
    public string Slug { get; private set; } = string.Empty;
    public string? Name { get; private set; }
    public string? Logo { get; private set; }
    public string? Ticker { get; private set; }
    public DateOnly? NextUnlockDate { get; private set; }

    /// <summary>Human-readable unlock amount including ticker, e.g. "15.8M SOSO".</summary>
    public string? UnlockAmount { get; private set; }

    /// <summary>USD value of the unlocked tokens (e.g. 6_650_000 for "$6.65M").</summary>
    public decimal? UnlockValueUsd { get; private set; }

    /// <summary>Vesting round names included in this event (comma-separated).</summary>
    public string? RoundName { get; private set; }

    /// <summary>Percentage of total supply currently circulating (e.g. 20.7 for "20.7%").</summary>
    public decimal? CirculationPercent { get; private set; }

    public decimal? PriceChange24h { get; private set; }
    public decimal? PriceChange7d { get; private set; }
    public decimal? PriceChange30d { get; private set; }
    public decimal? PriceChange1y { get; private set; }

    /// <summary>24-hour trading volume in USD.</summary>
    public decimal? Volume24hUsd { get; private set; }

    /// <summary>Unlock size as percentage of current circulating supply.</summary>
    public decimal? UnlockPercent { get; private set; }

    public static ChainBrokerUnlock Create(ChainBrokerUnlockSnapshot snapshot)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(snapshot.Slug);
        var entity = new ChainBrokerUnlock { Id = Guid.NewGuid() };
        Apply(entity, snapshot);
        return entity;
    }

    public void Update(ChainBrokerUnlockSnapshot snapshot) => Apply(this, snapshot);

    private static void Apply(ChainBrokerUnlock e, ChainBrokerUnlockSnapshot s)
    {
        e.Slug = s.Slug.Trim();
        e.Name = s.Name;
        e.Logo = s.Logo;
        e.Ticker = s.Ticker;
        e.NextUnlockDate = ChainBrokerValueParser.ParseDate(s.NextUnlock);
        e.UnlockAmount = s.UnlockAmount;
        e.UnlockValueUsd = ChainBrokerValueParser.ParseUsd(s.UnlockValue);
        e.RoundName = s.RoundName;
        e.CirculationPercent = ChainBrokerValueParser.ParsePercent(s.Circulation);
        e.PriceChange24h = ChainBrokerValueParser.ParsePercent(s.PriceChange24h);
        e.PriceChange7d = ChainBrokerValueParser.ParsePercent(s.PriceChange7d);
        e.PriceChange30d = ChainBrokerValueParser.ParsePercent(s.PriceChange30d);
        e.PriceChange1y = ChainBrokerValueParser.ParsePercent(s.PriceChange1y);
        e.Volume24hUsd = ChainBrokerValueParser.ParseUsd(s.Volume24h);
        e.UnlockPercent = ChainBrokerValueParser.ParsePercent(s.Percent);
    }
}

/// <summary>
/// Raw string values from the ChainBroker unlocks API.
/// Passed to <see cref="ChainBrokerUnlock.Create"/> or <see cref="ChainBrokerUnlock.Update"/>.
/// </summary>
public sealed record ChainBrokerUnlockSnapshot
{
    public required string Slug { get; init; }
    public string? Name { get; init; }
    public string? Logo { get; init; }
    public string? Ticker { get; init; }
    public string? NextUnlock { get; init; }
    public string? UnlockAmount { get; init; }
    public string? UnlockValue { get; init; }
    public string? RoundName { get; init; }
    public string? Circulation { get; init; }
    public string? PriceChange24h { get; init; }
    public string? PriceChange7d { get; init; }
    public string? PriceChange30d { get; init; }
    public string? PriceChange1y { get; init; }
    public string? Volume24h { get; init; }
    public string? Percent { get; init; }
}
