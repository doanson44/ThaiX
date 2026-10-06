using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteExpenseTag;

/// <summary>
/// Handler for DeleteExpenseTagCommand.
/// </summary>
public sealed class DeleteExpenseTagCommandHandler : IRequestHandler<DeleteExpenseTagCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteExpenseTagCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteExpenseTagCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.ExpenseTags.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Tag '{request.Id}' not found.");

        entity.SoftDelete();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
