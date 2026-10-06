using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetTransferList;

/// <summary>
/// Handler for GetTransferListQuery.
/// </summary>
public sealed class GetTransferListQueryHandler : IRequestHandler<GetTransferListQuery, PagedResult<ExpenseTransferDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetTransferListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ExpenseTransferDto>> Handle(GetTransferListQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.Transfers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x => (x.Note ?? string.Empty).ToLower().Contains(s));
        }

        query = request.SortDescending
            ? query.OrderByDescending(x => x.TransferredOn)
            : query.OrderBy(x => x.TransferredOn);

        return await query
            .Select(x => new ExpenseTransferDto
            {
                Id = x.Id,
                SourceWalletId = x.SourceWalletId,
                TargetWalletId = x.TargetWalletId,
                Amount = x.Amount,
                TransferredOn = x.TransferredOn,
                Note = x.Note
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
