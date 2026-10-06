using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseCategoryDetail;

/// <summary>
/// Handler for GetExpenseCategoryDetailQuery.
/// </summary>
public sealed class GetExpenseCategoryDetailQueryHandler : IRequestHandler<GetExpenseCategoryDetailQuery, ExpenseCategoryDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetExpenseCategoryDetailQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExpenseCategoryDto> Handle(GetExpenseCategoryDetailQuery request, CancellationToken cancellationToken)
    {
        var dto = await _dbContext.ExpenseCategories
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new ExpenseCategoryDto
            {
                Id = x.Id,
                Name = x.Name,
                CategoryType = x.CategoryType,
                ParentCategoryId = x.ParentCategoryId
            })
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Category '{request.Id}' not found.");
    }
}
