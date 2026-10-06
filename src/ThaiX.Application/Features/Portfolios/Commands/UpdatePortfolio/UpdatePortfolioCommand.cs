using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.Portfolios;

namespace ThaiX.Application.Features.Portfolios.Commands.UpdatePortfolio;

[InvalidateCache(CacheGroups.Portfolios)]
public sealed record UpdatePortfolioCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required PortfolioType PortfolioType { get; init; }
    public string? Description { get; init; }
}
