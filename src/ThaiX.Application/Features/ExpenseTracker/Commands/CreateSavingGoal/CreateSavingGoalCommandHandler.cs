using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateSavingGoal;

/// <summary>
/// Handler for CreateSavingGoalCommand.
/// </summary>
public sealed class CreateSavingGoalCommandHandler : IRequestHandler<CreateSavingGoalCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateSavingGoalCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateSavingGoalCommand request, CancellationToken cancellationToken)
    {
        var entity = SavingGoal.Create(request.WalletId, request.Name, request.TargetAmount, request.CurrentAmount, request.TargetDate);
        _dbContext.SavingGoals.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
