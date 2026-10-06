using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteTransactionTag;

/// <summary>
/// Handler for DeleteTransactionTagCommand.
/// </summary>
public sealed class DeleteTransactionTagCommandHandler : IRequestHandler<DeleteTransactionTagCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteTransactionTagCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(DeleteTransactionTagCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TransactionTags.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Transaction tag '{request.Id}' not found.");

        entity.SoftDelete();
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
