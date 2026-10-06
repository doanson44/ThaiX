using ThaiX.Application.Common.Models.MarketScanner;

namespace ThaiX.Infrastructure.Services.MarketScanner;

internal sealed class RollingSnapshotBuffer
{
    private readonly List<MarketTickerSnapshot> _entries = [];
    private readonly object _lock = new();

    public void Append(MarketTickerSnapshot snapshot)
    {
        lock (_lock)
        {
            _entries.Add(snapshot);
        }
    }

    public IReadOnlyList<MarketTickerSnapshot> GetSnapshots(TimeSpan window, DateTimeOffset now)
    {
        var cutoff = now - window;
        lock (_lock)
        {
            return _entries
                .Where(e => e.CapturedAtUtc >= cutoff)
                .OrderBy(e => e.CapturedAtUtc)
                .ToList();
        }
    }

    public void RemoveExpired(TimeSpan retention, DateTimeOffset now)
    {
        var cutoff = now - retention;
        lock (_lock)
        {
            _entries.RemoveAll(e => e.CapturedAtUtc < cutoff);
        }
    }
}
