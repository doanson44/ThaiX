using ThaiX.Application.Common.Interfaces.MarketScanner;
using ThaiX.Application.Common.Models.MarketScanner;
using ThaiX.Application.Features.MarketScanner.Queries.GetMarketScannerRules;
using ThaiX.Domain.Aggregates.MarketScanner;

namespace ThaiX.Infrastructure.Services.MarketScanner.Detectors;

internal sealed class FundingFlipDetector : IMarketSignalDetector
{
    public MarketSignalType SignalType => MarketSignalType.FundingFlip;

    public IReadOnlyCollection<MarketSignalResult> Detect(
        MarketScannerRuleListItemDto rule,
        IReadOnlyList<MarketTickerSnapshot> snapshots)
    {
        if (snapshots.Count < 2)
            return [];

        var oldest = snapshots[0];
        var latest = snapshots[^1];

        bool wasPositive = oldest.FundingRate > 0m;
        bool isPositive = latest.FundingRate > 0m;
        bool wasNegative = oldest.FundingRate < 0m;
        bool isNegative = latest.FundingRate < 0m;

        bool flipped = (wasNegative && isPositive) || (wasPositive && isNegative);
        if (!flipped)
            return [];

        return
        [
            new MarketSignalResult
            {
                Symbol = latest.Symbol,
                Type = MarketSignalType.FundingFlip,
                Value = latest.FundingRate,
                Message = $"Funding rate flipped from {oldest.FundingRate:F6} to {latest.FundingRate:F6}",
                TriggeredAtUtc = latest.CapturedAtUtc
            }
        ];
    }
}
