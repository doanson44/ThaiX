using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Extensions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseCategoryList;

/// <summary>
/// Handler for GetExpenseCategoryListQuery.
/// </summary>
public sealed class GetExpenseCategoryListQueryHandler : IRequestHandler<GetExpenseCategoryListQuery, PagedResult<ExpenseCategoryDto>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetExpenseCategoryListQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PagedResult<ExpenseCategoryDto>> Handle(GetExpenseCategoryListQuery request, CancellationToken cancellationToken)
    {
        var query = _dbContext.ExpenseCategories.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var s = request.Search.Trim().ToLowerInvariant();
            query = query.Where(x => x.Name.ToLower().Contains(s));
        }

        query = request.SortDescending
            ? query.OrderByDescending(x => x.Name)
            : query.OrderBy(x => x.Name);

        return await query
            .Select(x => new ExpenseCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                CategoryType = x.CategoryType,
                ParentCategoryId = x.ParentCategoryId
            })
            .ToPagedListAsync(request, cancellationToken);
    }
}
