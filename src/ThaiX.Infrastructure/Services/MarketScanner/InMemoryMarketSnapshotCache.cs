using System.Collections.Concurrent;
using ThaiX.Application.Common.Interfaces.MarketScanner;
using ThaiX.Application.Common.Models.MarketScanner;

namespace ThaiX.Infrastructure.Services.MarketScanner;

internal sealed class InMemoryMarketSnapshotCache : IMarketSnapshotCache
{
    private readonly ConcurrentDictionary<string, RollingSnapshotBuffer> _buffers = new();

    public void AddSnapshot(MarketTickerSnapshot snapshot)
    {
        var buffer = _buffers.GetOrAdd(snapshot.Symbol, _ => new RollingSnapshotBuffer());
        buffer.Append(snapshot);
    }

    public IReadOnlyList<MarketTickerSnapshot> GetSnapshots(string symbol, TimeSpan window)
    {
        if (!_buffers.TryGetValue(symbol, out var buffer))
            return [];

        return buffer.GetSnapshots(window, DateTimeOffset.UtcNow);
    }

    public void Cleanup(TimeSpan retention)
    {
        var now = DateTimeOffset.UtcNow;
        foreach (var buffer in _buffers.Values)
        {
            buffer.RemoveExpired(retention, now);
        }
    }
}
