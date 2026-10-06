using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.JsonBins.Commands.ExpireJsonBin;

[InvalidateCache(CacheGroups.JsonBins)]
public sealed record ExpireJsonBinCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
