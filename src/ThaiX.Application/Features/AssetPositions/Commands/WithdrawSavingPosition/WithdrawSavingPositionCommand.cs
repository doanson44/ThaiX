using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.AssetPositions.Commands.WithdrawSavingPosition;

[InvalidateCache(CacheGroups.SavingPositions)]
public sealed record WithdrawSavingPositionCommand(Guid Id, DateOnly WithdrawalDate) : IAppCommand<Unit>;
