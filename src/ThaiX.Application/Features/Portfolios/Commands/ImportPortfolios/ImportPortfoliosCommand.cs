using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.Portfolios.Commands.ImportPortfolios;

[InvalidateCache(CacheGroups.Portfolios)]
public sealed record ImportPortfoliosCommand : IAppCommand<ImportResult>
{
    public required Stream CsvStream { get; init; }
}
