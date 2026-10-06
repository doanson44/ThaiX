using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.AssetPositions;

namespace ThaiX.Application.Features.AssetPositions.Commands.AddCryptoTransaction;

public sealed class AddCryptoTransactionCommandHandler : IRequestHandler<AddCryptoTransactionCommand, Unit>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public AddCryptoTransactionCommandHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<Unit> Handle(AddCryptoTransactionCommand request, CancellationToken cancellationToken)
    {
        var position = await _context.CryptoPositions
                        .Include(x => x.Transactions)
                        .FirstOrDefaultAsync(x =>
                            x.Id == request.PositionId &&
                            !x.IsDeleted &&
                            x.Portfolio.OwnerId == _currentUser.UserId,
                            cancellationToken)
                        ?? throw new OperationFailedException(
                            ErrorCodes.RESOURCE_NOT_FOUND,
                            $"Position '{request.PositionId}' not found.");

        if (request.TransactionType == TransactionType.Buy)
            position.AddBuy(request.Quantity, request.Price, request.Fee, request.TransactedAt, request.Note, request.ExternalRef);
        else if (request.TransactionType == TransactionType.Sell)
            position.AddSell(request.Quantity, request.Price, request.Fee, request.TransactedAt, request.Note, request.ExternalRef);
        else
            throw new InvalidOperationException($"Transaction type {request.TransactionType} is not supported for crypto positions.");

        await _context.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
