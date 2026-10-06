using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseTagList;

/// <summary>
/// Handler for GetExpenseTagListQuery.
/// </summary>
public sealed class GetExpenseTagListQueryHandler : IRequestHandler<GetExpenseTagListQuery, PagedResult<ExpenseTagDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetExpenseTagListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ExpenseTagDto>> Handle(GetExpenseTagListQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.ExpenseTags.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(s));
        }

        query = request.SortDescending
            ? query.OrderByDescending(x => x.Name)
            : query.OrderBy(x => x.Name);

        return await query
            .Select(x => new ExpenseTagDto
            {
                Id = x.Id,
                Name = x.Name,
                ColorHex = x.ColorHex
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
