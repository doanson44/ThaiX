using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetBudgetDetail;

/// <summary>
/// Handler for GetBudgetDetailQuery.
/// </summary>
public sealed class GetBudgetDetailQueryHandler : IRequestHandler<GetBudgetDetailQuery, ExpenseBudgetDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetBudgetDetailQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExpenseBudgetDto> Handle(GetBudgetDetailQuery request, CancellationToken cancellationToken)
    {
        var dto = await _dbContext.Budgets
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new ExpenseBudgetDto
            {
                Id = x.Id,
                CategoryId = x.CategoryId,
                Period = x.Period,
                Amount = x.Amount,
                StartDate = x.StartDate,
                EndDate = x.EndDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Budget '{request.Id}' not found.");
    }
}
