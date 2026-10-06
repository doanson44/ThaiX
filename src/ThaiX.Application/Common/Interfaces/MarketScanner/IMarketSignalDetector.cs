using ThaiX.Application.Common.Models.MarketScanner;
using ThaiX.Application.Features.MarketScanner.Queries.GetMarketScannerRules;
using ThaiX.Domain.Aggregates.MarketScanner;

namespace ThaiX.Application.Common.Interfaces.MarketScanner;

public interface IMarketSignalDetector
{
    MarketSignalType SignalType { get; }

    IReadOnlyCollection<MarketSignalResult> Detect(
        MarketScannerRuleListItemDto rule,
        IReadOnlyList<MarketTickerSnapshot> snapshots);
}
