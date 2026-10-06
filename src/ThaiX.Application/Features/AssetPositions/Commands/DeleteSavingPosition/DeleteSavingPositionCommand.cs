using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.AssetPositions.Commands.DeleteSavingPosition;

[InvalidateCache(CacheGroups.SavingPositions)]
public sealed record DeleteSavingPositionCommand(Guid Id) : IAppCommand<Unit>;
