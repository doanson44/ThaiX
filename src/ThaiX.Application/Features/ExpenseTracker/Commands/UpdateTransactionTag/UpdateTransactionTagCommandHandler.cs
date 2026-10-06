using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateTransactionTag;

/// <summary>
/// Handler for UpdateTransactionTagCommand.
/// </summary>
public sealed class UpdateTransactionTagCommandHandler : IRequestHandler<UpdateTransactionTagCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateTransactionTagCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateTransactionTagCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.TransactionTags.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Transaction tag '{request.Id}' not found.");

        entity.Update(request.TransactionId, request.TagId);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
