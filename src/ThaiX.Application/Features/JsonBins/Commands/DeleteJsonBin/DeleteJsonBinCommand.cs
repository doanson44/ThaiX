using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.JsonBins.Commands.DeleteJsonBin;

[InvalidateCache(CacheGroups.JsonBins)]
public sealed record DeleteJsonBinCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
