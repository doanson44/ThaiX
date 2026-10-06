using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateBudget;

/// <summary>
/// Command to create a budget.
/// </summary>
[InvalidateCache(CacheGroups.Budgets)]
public sealed record CreateBudgetCommand : IAppCommand<Guid>
{
    public required Guid CategoryId { get; init; }
    public required BudgetPeriod Period { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
}
