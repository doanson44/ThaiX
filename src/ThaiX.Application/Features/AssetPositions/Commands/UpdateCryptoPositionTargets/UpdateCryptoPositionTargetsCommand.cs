using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.AssetPositions.Commands.UpdateCryptoPositionTargets;

[InvalidateCache(CacheGroups.CryptoPositions)]
public sealed record UpdateCryptoPositionTargetsCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public decimal? TargetPrice { get; init; }
    public decimal? StopLoss { get; init; }
    public string? Note { get; init; }
}
