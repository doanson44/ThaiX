using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.MarketScanner;

namespace ThaiX.Application.Features.MarketScanner.Commands.CreateMarketScannerRule;

/// <summary>
/// Command to create a new market scanner rule.
/// </summary>
[InvalidateCache(CacheGroups.MarketScanner)]
public sealed record CreateMarketScannerRuleCommand : IAppCommand<Guid>
{
    public required string Name { get; init; }
    public required MarketSignalType SignalType { get; init; }
    public required MarketTimeWindow Window { get; init; }
    public decimal? Threshold { get; init; }
}
