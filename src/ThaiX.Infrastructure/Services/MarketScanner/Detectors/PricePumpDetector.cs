using ThaiX.Application.Common.Interfaces.MarketScanner;
using ThaiX.Application.Common.Models.MarketScanner;
using ThaiX.Application.Features.MarketScanner.Queries.GetMarketScannerRules;
using ThaiX.Domain.Aggregates.MarketScanner;

namespace ThaiX.Infrastructure.Services.MarketScanner.Detectors;

internal sealed class PricePumpDetector : IMarketSignalDetector
{
    public MarketSignalType SignalType => MarketSignalType.PricePump;

    public IReadOnlyCollection<MarketSignalResult> Detect(
        MarketScannerRuleListItemDto rule,
        IReadOnlyList<MarketTickerSnapshot> snapshots)
    {
        if (snapshots.Count < 2 || !rule.Threshold.HasValue)
            return [];

        var oldest = snapshots[0];
        var latest = snapshots[^1];

        if (oldest.Price == 0m)
            return [];

        var deltaPercent = (latest.Price - oldest.Price) / oldest.Price * 100m;
        var threshold = rule.Threshold.Value;
        if (Math.Abs(deltaPercent) < threshold)
            return [];

        var direction = deltaPercent >= 0 ? "pumped" : "dumped";

        return
        [
            new MarketSignalResult
            {
                Symbol = latest.Symbol,
                Type = MarketSignalType.PricePump,
                Value = deltaPercent,
                Message = $"Price {direction} {deltaPercent:F2}% (threshold {threshold:F2}%) over {(int)rule.Window} min",
                TriggeredAtUtc = latest.CapturedAtUtc
            }
        ];
    }
}
