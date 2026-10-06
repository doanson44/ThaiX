using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteBudget;

/// <summary>
/// Handler for DeleteBudgetCommand.
/// </summary>
public sealed class DeleteBudgetCommandHandler : IRequestHandler<DeleteBudgetCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteBudgetCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteBudgetCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.Budgets.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Budget '{request.Id}' not found.");

        entity.SoftDelete();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
