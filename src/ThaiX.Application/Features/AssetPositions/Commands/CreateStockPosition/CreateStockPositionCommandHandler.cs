using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.AssetPositions;

namespace ThaiX.Application.Features.AssetPositions.Commands.CreateStockPosition;

public sealed class CreateStockPositionCommandHandler : IRequestHandler<CreateStockPositionCommand, Guid>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public CreateStockPositionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Guid> Handle(CreateStockPositionCommand request, CancellationToken cancellationToken)
    {
        var portfolioExists = await _context.Portfolios
            .AnyAsync(p => p.Id == request.PortfolioId && p.OwnerId == _currentUser.UserId && !p.IsDeleted, cancellationToken);

        if (!portfolioExists)
            throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Portfolio '{request.PortfolioId}' not found.");

        var position = StockPosition.Create(
            request.PortfolioId,
            request.Symbol,
            request.Exchange,
            request.TargetPrice,
            request.StopLoss,
            request.Note);

        position.AddBuy(
            request.Quantity,
            request.Price,
            request.Fee,
            request.TransactedAt,
            request.TransactionNote,
            request.ExternalRef);

        _context.StockPositions.Add(position);
        await _context.SaveChangesAsync(cancellationToken);

        return position.Id;
    }
}
