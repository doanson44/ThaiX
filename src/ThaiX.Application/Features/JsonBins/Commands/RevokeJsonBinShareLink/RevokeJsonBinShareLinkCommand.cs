using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.JsonBins.Commands.RevokeJsonBinShareLink;

[InvalidateCache(CacheGroups.JsonBins)]
public sealed record RevokeJsonBinShareLinkCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
