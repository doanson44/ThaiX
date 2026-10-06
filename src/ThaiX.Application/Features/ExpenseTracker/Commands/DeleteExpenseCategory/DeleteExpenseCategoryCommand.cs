using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteExpenseCategory;

/// <summary>
/// Command to soft-delete an expense category.
/// </summary>
[InvalidateCache(CacheGroups.ExpenseCategories)]
public sealed record DeleteExpenseCategoryCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
