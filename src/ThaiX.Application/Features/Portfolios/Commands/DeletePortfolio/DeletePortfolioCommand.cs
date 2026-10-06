using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Portfolios.Commands.DeletePortfolio;

[InvalidateCache(CacheGroups.Portfolios)]
public sealed record DeletePortfolioCommand(Guid Id) : IAppCommand<Unit>;
