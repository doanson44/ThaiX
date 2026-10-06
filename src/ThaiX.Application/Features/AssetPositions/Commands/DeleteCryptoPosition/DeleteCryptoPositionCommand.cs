using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.AssetPositions.Commands.DeleteCryptoPosition;

[InvalidateCache(CacheGroups.CryptoPositions)]
public sealed record DeleteCryptoPositionCommand(Guid Id) : IAppCommand<Unit>;
