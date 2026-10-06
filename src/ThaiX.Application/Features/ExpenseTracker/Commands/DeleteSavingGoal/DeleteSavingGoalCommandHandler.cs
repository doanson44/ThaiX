using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteSavingGoal;

/// <summary>
/// Handler for DeleteSavingGoalCommand.
/// </summary>
public sealed class DeleteSavingGoalCommandHandler : IRequestHandler<DeleteSavingGoalCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteSavingGoalCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteSavingGoalCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.SavingGoals.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Saving goal '{request.Id}' not found.");

        entity.SoftDelete();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
