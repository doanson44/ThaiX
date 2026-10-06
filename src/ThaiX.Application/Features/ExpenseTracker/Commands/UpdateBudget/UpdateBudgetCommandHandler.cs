using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateBudget;

/// <summary>
/// Handler for UpdateBudgetCommand.
/// </summary>
public sealed class UpdateBudgetCommandHandler : IRequestHandler<UpdateBudgetCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateBudgetCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateBudgetCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Budgets.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Budget '{request.Id}' not found.");

        entity.Update(request.CategoryId, request.Period, request.Amount, request.StartDate, request.EndDate);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
