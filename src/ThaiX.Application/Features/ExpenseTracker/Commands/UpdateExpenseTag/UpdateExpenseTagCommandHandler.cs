using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateExpenseTag;

/// <summary>
/// Handler for UpdateExpenseTagCommand.
/// </summary>
public sealed class UpdateExpenseTagCommandHandler : IRequestHandler<UpdateExpenseTagCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateExpenseTagCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateExpenseTagCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.ExpenseTags.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Tag '{request.Id}' not found.");

        entity.Update(request.Name, request.ColorHex);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
