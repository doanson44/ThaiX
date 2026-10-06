using MediatR;
using ThaiX.Application.Common.Interfaces;
using ExpenseCategory = ThaiX.Domain.Aggregates.ExpenseTracker.Category;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateExpenseCategory;

/// <summary>
/// Handler for CreateExpenseCategoryCommand.
/// </summary>
public sealed class CreateExpenseCategoryCommandHandler : IRequestHandler<CreateExpenseCategoryCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateExpenseCategoryCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateExpenseCategoryCommand request, CancellationToken cancellationToken)
    {
        var entity = ExpenseCategory.Create(request.Name, request.CategoryType, request.ParentCategoryId);
        _dbContext.ExpenseCategories.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return entity.Id;
    }
}
