using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateExpenseCategory;

/// <summary>
/// Handler for UpdateExpenseCategoryCommand.
/// </summary>
public sealed class UpdateExpenseCategoryCommandHandler : IRequestHandler<UpdateExpenseCategoryCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UpdateExpenseCategoryCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UpdateExpenseCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = await _dbContext.ExpenseCategories.FirstOrDefaultAsync(x => x.Id == request.Id && !x.IsDeleted, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, $"Category '{request.Id}' not found.");

        entity.Update(request.Name, request.CategoryType, request.ParentCategoryId);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
