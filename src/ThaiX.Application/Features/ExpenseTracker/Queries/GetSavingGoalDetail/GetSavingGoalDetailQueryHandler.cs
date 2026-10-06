using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetSavingGoalDetail;

/// <summary>
/// Handler for GetSavingGoalDetailQuery.
/// </summary>
public sealed class GetSavingGoalDetailQueryHandler : IRequestHandler<GetSavingGoalDetailQuery, ExpenseSavingGoalDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetSavingGoalDetailQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExpenseSavingGoalDto> Handle(GetSavingGoalDetailQuery request, CancellationToken cancellationToken)
    {
        var dto = await _dbContext.SavingGoals
            .AsNoTracking()
            .Where(x => x.Id == request.Id)
            .Select(x => new ExpenseSavingGoalDto
            {
                Id = x.Id,
                WalletId = x.WalletId,
                Name = x.Name,
                TargetAmount = x.TargetAmount,
                CurrentAmount = x.CurrentAmount,
                TargetDate = x.TargetDate
            })
            .FirstOrDefaultAsync(cancellationToken);

        return dto ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Saving goal '{request.Id}' not found.");
    }
}
