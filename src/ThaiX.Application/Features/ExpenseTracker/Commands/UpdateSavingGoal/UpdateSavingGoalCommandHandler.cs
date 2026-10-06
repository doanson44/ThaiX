using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateSavingGoal;

/// <summary>
/// Handler for UpdateSavingGoalCommand.
/// </summary>
public sealed class UpdateSavingGoalCommandHandler : IRequestHandler<UpdateSavingGoalCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateSavingGoalCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateSavingGoalCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.SavingGoals.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Saving goal '{request.Id}' not found.");

        entity.Update(request.WalletId, request.Name, request.TargetAmount, request.CurrentAmount, request.TargetDate);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
