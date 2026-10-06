using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.AssetPositions.Commands.DeleteStockPosition;

[InvalidateCache(CacheGroups.StockPositions)]
public sealed record DeleteStockPositionCommand(Guid Id) : IAppCommand<Unit>;
