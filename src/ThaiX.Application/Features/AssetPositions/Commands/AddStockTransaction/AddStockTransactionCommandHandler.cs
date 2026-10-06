using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.AssetPositions;

namespace ThaiX.Application.Features.AssetPositions.Commands.AddStockTransaction;

public sealed class AddStockTransactionCommandHandler : IRequestHandler<AddStockTransactionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddStockTransactionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(AddStockTransactionCommand request, CancellationToken cancellationToken)
    {
        var position = await _context.StockPositions
            .Include(p => p.Transactions)
            .FirstOrDefaultAsync(p =>
                p.Id == request.PositionId &&
                !p.IsDeleted &&
                p.Portfolio.OwnerId == _currentUser.UserId,
                cancellationToken)
            ?? throw new OperationFailedException(
                ErrorCodes.RESOURCE_NOT_FOUND,
                $"Position '{request.PositionId}' not found.");

        switch (request.TransactionType)
        {
            case TransactionType.Buy:
                position.AddBuy(
                    request.Quantity,
                    request.Price,
                    request.Fee,
                    request.TransactedAt,
                    request.Note,
                    request.ExternalRef);
                break;

            case TransactionType.Sell:
                position.AddSell(
                    request.Quantity,
                    request.Price,
                    request.Fee,
                    request.TransactedAt,
                    request.Note,
                    request.ExternalRef);
                break;

            default:
                throw new InvalidOperationException(
                    $"Transaction type {request.TransactionType} is not supported for stock positions.");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
