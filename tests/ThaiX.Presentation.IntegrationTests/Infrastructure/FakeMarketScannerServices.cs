using ThaiX.Application.Common.Interfaces.MarketScanner;
using ThaiX.Application.Common.Models.MarketScanner;
using ThaiX.Application.Features.MarketScanner.Queries.GetMarketScannerRules;
using ThaiX.Domain.Aggregates.MarketScanner;

namespace ThaiX.Presentation.IntegrationTests.Infrastructure;

public sealed class FakeMarketSnapshotCache : IMarketSnapshotCache
{
    private readonly Dictionary<string, List<MarketTickerSnapshot>> _store =
        new(StringComparer.OrdinalIgnoreCase);

    public void AddSnapshot(MarketTickerSnapshot snapshot)
    {
        if (!_store.TryGetValue(snapshot.Symbol, out var snapshots))
        {
            snapshots = [];
            _store[snapshot.Symbol] = snapshots;
        }

        snapshots.Add(snapshot);
    }

    public IReadOnlyList<MarketTickerSnapshot> GetSnapshots(string symbol, TimeSpan window)
    {
        if (!_store.TryGetValue(symbol, out var snapshots))
        {
            return [];
        }

        var cutoff = DateTimeOffset.UtcNow.Subtract(window);
        return snapshots.Where(x => x.CapturedAtUtc >= cutoff).ToArray();
    }

    public void Cleanup(TimeSpan retention)
    {
        var cutoff = DateTimeOffset.UtcNow.Subtract(retention);
        foreach (var key in _store.Keys.ToArray())
        {
            _store[key] = _store[key].Where(x => x.CapturedAtUtc >= cutoff).ToList();
        }
    }
}

public sealed class FakeMarketSignalCooldownService : IMarketSignalCooldownService
{
    private readonly HashSet<string> _triggered = new(StringComparer.OrdinalIgnoreCase);

    public bool IsOnCooldown(string symbol, MarketSignalType signalType)
        => _triggered.Contains(BuildKey(symbol, signalType));

    public void MarkTriggered(string symbol, MarketSignalType signalType)
        => _triggered.Add(BuildKey(symbol, signalType));

    private static string BuildKey(string symbol, MarketSignalType signalType)
        => $"{symbol}:{signalType}";
}

public sealed class FakeMarketSignalDetector : IMarketSignalDetector
{
    public MarketSignalType SignalType => MarketSignalType.PricePump;

    public IReadOnlyCollection<MarketSignalResult> Detect(
        MarketScannerRuleListItemDto rule,
        IReadOnlyList<MarketTickerSnapshot> snapshots)
    {
        _ = rule;
        _ = snapshots;
        return [];
    }
}
