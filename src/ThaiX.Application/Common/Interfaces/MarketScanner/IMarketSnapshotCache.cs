using ThaiX.Application.Common.Models.MarketScanner;

namespace ThaiX.Application.Common.Interfaces.MarketScanner;

public interface IMarketSnapshotCache
{
    void AddSnapshot(MarketTickerSnapshot snapshot);

    IReadOnlyList<MarketTickerSnapshot> GetSnapshots(
        string symbol,
        TimeSpan window);

    void Cleanup(TimeSpan retention);
}
