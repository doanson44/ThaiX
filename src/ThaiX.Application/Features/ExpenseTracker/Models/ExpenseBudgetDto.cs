using ThaiX.Domain.Aggregates.ExpenseTracker;
namespace ThaiX.Application.Features.ExpenseTracker.Models;

/// <summary>
/// DTO for budget.
/// </summary>
public sealed record ExpenseBudgetDto
{
    public required Guid Id { get; init; }
    public required Guid CategoryId { get; init; }
    public required BudgetPeriod Period { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime StartDate { get; init; }
    public required DateTime EndDate { get; init; }
}
