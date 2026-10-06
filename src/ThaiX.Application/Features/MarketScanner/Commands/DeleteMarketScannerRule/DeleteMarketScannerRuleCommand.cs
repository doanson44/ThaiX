using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.MarketScanner.Commands.DeleteMarketScannerRule;

/// <summary>
/// Command to soft-delete a market scanner rule.
/// </summary>
[InvalidateCache(CacheGroups.MarketScanner)]
public sealed record DeleteMarketScannerRuleCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
