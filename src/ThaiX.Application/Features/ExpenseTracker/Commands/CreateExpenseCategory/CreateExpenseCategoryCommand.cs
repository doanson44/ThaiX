using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateExpenseCategory;

/// <summary>
/// Command to create an expense category.
/// </summary>
[InvalidateCache(CacheGroups.ExpenseCategories)]
public sealed record CreateExpenseCategoryCommand : IAppCommand<Guid>
{
    public required string Name { get; init; }
    public required CategoryType CategoryType { get; init; }
    public Guid? ParentCategoryId { get; init; }
}
