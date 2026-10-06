using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteBudget;

/// <summary>
/// Command to soft-delete a budget.
/// </summary>
[InvalidateCache(CacheGroups.Budgets)]
public sealed record DeleteBudgetCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
