using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateExpenseCategory;

/// <summary>
/// Command to update an expense category.
/// </summary>
[InvalidateCache(CacheGroups.ExpenseCategories)]
public sealed record UpdateExpenseCategoryCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required CategoryType CategoryType { get; init; }
    public Guid? ParentCategoryId { get; init; }
}
