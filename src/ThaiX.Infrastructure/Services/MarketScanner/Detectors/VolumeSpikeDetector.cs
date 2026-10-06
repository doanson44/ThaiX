using ThaiX.Application.Common.Interfaces.MarketScanner;
using ThaiX.Application.Common.Models.MarketScanner;
using ThaiX.Application.Features.MarketScanner.Queries.GetMarketScannerRules;
using ThaiX.Domain.Aggregates.MarketScanner;

namespace ThaiX.Infrastructure.Services.MarketScanner.Detectors;

internal sealed class VolumeSpikeDetector : IMarketSignalDetector
{
    public MarketSignalType SignalType => MarketSignalType.VolumeSpike;

    public IReadOnlyCollection<MarketSignalResult> Detect(
        MarketScannerRuleListItemDto rule,
        IReadOnlyList<MarketTickerSnapshot> snapshots)
    {
        if (snapshots.Count < 2 || !rule.Threshold.HasValue)
            return [];

        var oldest = snapshots[0];
        var latest = snapshots[^1];

        if (oldest.Volume24h == 0m)
            return [];

        var multiple = latest.Volume24h / oldest.Volume24h;
        var threshold = rule.Threshold.Value;
        if (multiple < threshold)
            return [];

        return
        [
            new MarketSignalResult
            {
                Symbol = latest.Symbol,
                Type = MarketSignalType.VolumeSpike,
                Value = multiple,
                Message = $"Volume spiked {multiple:F2}x (threshold {threshold}x) from {oldest.Volume24h:F0} to {latest.Volume24h:F0}",
                TriggeredAtUtc = latest.CapturedAtUtc
            }
        ];
    }
}
