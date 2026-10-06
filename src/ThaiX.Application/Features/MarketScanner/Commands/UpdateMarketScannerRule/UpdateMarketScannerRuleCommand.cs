using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.MarketScanner;

namespace ThaiX.Application.Features.MarketScanner.Commands.UpdateMarketScannerRule;

/// <summary>
/// Command to update an existing market scanner rule.
/// </summary>
[InvalidateCache(CacheGroups.MarketScanner)]
public sealed record UpdateMarketScannerRuleCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required MarketTimeWindow Window { get; init; }
    public decimal? Threshold { get; init; }
    public required bool IsEnabled { get; init; }
}
