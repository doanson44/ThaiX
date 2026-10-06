using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Aggregates.AssetPositions;

namespace ThaiX.Application.Features.AssetPositions.Queries.GetPositionTransactions;

public sealed class GetPositionTransactionsQueryHandler
    : IRequestHandler<GetPositionTransactionsQuery, PagedResult<PositionTransactionDto>>
{
    private readonly IApplicationDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public GetPositionTransactionsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<PositionTransactionDto>> Handle(
    GetPositionTransactionsQuery request,
    CancellationToken cancellationToken)
    {
        var query = _context.PositionTransactions
            .AsNoTracking()
            .Where(t => t.PositionId == request.PositionId && t.AssetType == request.AssetType);

        query = request.AssetType switch
        {
            PositionAssetType.Crypto => query.Where(t =>
                _context.CryptoPositions.Any(p =>
                    p.Id == t.PositionId &&
                    p.Portfolio.OwnerId == _currentUser.UserId)),

            PositionAssetType.Stock => query.Where(t =>
                _context.StockPositions.Any(p =>
                    p.Id == t.PositionId &&
                    p.Portfolio.OwnerId == _currentUser.UserId)),

            _ => throw new InvalidOperationException($"Unsupported asset type: {request.AssetType}.")
        };

        return await query
            .OrderByDescending(t => t.TransactedAt)
            .Select(t => new PositionTransactionDto
            {
                Id = t.Id,
                TransactionType = t.TransactionType,
                Quantity = t.Quantity,
                Price = t.Price,
                Fee = t.Fee,
                TransactedAt = t.TransactedAt,
                Note = t.Note,
                ExternalRef = t.ExternalRef
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
