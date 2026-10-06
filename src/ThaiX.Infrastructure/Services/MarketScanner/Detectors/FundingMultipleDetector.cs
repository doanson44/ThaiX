using ThaiX.Application.Common.Interfaces.MarketScanner;
using ThaiX.Application.Common.Models.MarketScanner;
using ThaiX.Application.Features.MarketScanner.Queries.GetMarketScannerRules;
using ThaiX.Domain.Aggregates.MarketScanner;

namespace ThaiX.Infrastructure.Services.MarketScanner.Detectors;

internal sealed class FundingMultipleDetector : IMarketSignalDetector
{
    public MarketSignalType SignalType => MarketSignalType.FundingMultiple;

    public IReadOnlyCollection<MarketSignalResult> Detect(
        MarketScannerRuleListItemDto rule,
        IReadOnlyList<MarketTickerSnapshot> snapshots)
    {
        if (snapshots.Count < 2 || !rule.Threshold.HasValue)
            return [];

        var oldest = snapshots[0];
        var latest = snapshots[^1];

        if (oldest.FundingRate == 0m)
            return [];

        // Only meaningful when both readings have the same sign
        if ((oldest.FundingRate > 0m) != (latest.FundingRate > 0m))
            return [];

        var multiple = latest.FundingRate / oldest.FundingRate;
        var threshold = rule.Threshold.Value;
        if (multiple < threshold)
            return [];

        return
        [
            new MarketSignalResult
            {
                Symbol = latest.Symbol,
                Type = MarketSignalType.FundingMultiple,
                Value = multiple,
                Message = $"Funding rate grew {multiple:F2}x (threshold {threshold}x) from {oldest.FundingRate:F6} to {latest.FundingRate:F6}",
                TriggeredAtUtc = latest.CapturedAtUtc
            }
        ];
    }
}
