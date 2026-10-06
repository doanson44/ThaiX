using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetTransactionTagList;

/// <summary>
/// Handler for GetTransactionTagListQuery.
/// </summary>
public sealed class GetTransactionTagListQueryHandler : IRequestHandler<GetTransactionTagListQuery, PagedResult<ExpenseTransactionTagDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetTransactionTagListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ExpenseTransactionTagDto>> Handle(GetTransactionTagListQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.TransactionTags.AsNoTracking().AsQueryable();
        query = request.SortDescending
            ? query.OrderByDescending(x => x.CreatedAt)
            : query.OrderBy(x => x.CreatedAt);

        return await query
            .Select(x => new ExpenseTransactionTagDto
            {
                Id = x.Id,
                TransactionId = x.TransactionId,
                TagId = x.TagId
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
