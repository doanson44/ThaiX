using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.Portfolios;

namespace ThaiX.Application.Features.Portfolios.Commands.CreatePortfolio;

[InvalidateCache(CacheGroups.Portfolios)]
public sealed record CreatePortfolioCommand : IAppCommand<Guid>
{
    public required string Name { get; init; }
    public required PortfolioType PortfolioType { get; init; }
    public string? Description { get; init; }
}
