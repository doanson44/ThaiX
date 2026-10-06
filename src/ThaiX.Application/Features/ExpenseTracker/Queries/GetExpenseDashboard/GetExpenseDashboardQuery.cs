using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.ExpenseTracker.Models;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseDashboard;

/// <summary>
/// Query to retrieve expense dashboard summary data.
/// </summary>
public sealed record GetExpenseDashboardQuery : IAppQuery<ExpenseDashboardDataDto>, ICacheableQuery
{
    public string CacheKey => CacheKeys.ExpenseDashboard.Summary();
    public TimeSpan? Expiration => TimeSpan.FromMinutes(5);
    public string CacheGroup => CacheGroups.ExpenseDashboard;
    public bool IsVersionedList => false;
}

/// <summary>
/// Category total amount for dashboard charts.
/// </summary>
public sealed record ExpenseCategoryAmountDto
{
    public required string CategoryName { get; init; }
    public required decimal Amount { get; init; }
}

/// <summary>
/// Monthly series point for dashboard charts.
/// </summary>
public sealed record ExpenseMonthlySeriesDto
{
    public required int Year { get; init; }
    public required int Month { get; init; }
    public required decimal Amount { get; init; }
}

/// <summary>
/// High-level dashboard summary metrics.
/// </summary>
public sealed record ExpenseDashboardSummaryDto
{
    public required decimal TotalAsset { get; init; }
    public required decimal IncomeToday { get; init; }
    public required decimal ExpenseToday { get; init; }
    public required decimal IncomeThisMonth { get; init; }
    public required decimal ExpenseThisMonth { get; init; }
}

/// <summary>
/// Full dashboard payload.
/// </summary>
public sealed record ExpenseDashboardDataDto
{
    public required ExpenseDashboardSummaryDto Summary { get; init; }
    public required decimal CashFlow { get; init; }
    public required IReadOnlyList<ExpenseCategoryAmountDto> ExpenseByCategory { get; init; }
    public required IReadOnlyList<ExpenseCategoryAmountDto> IncomeByCategory { get; init; }
    public required IReadOnlyList<ExpenseTransactionDto> RecentTransactions { get; init; }
    public required IReadOnlyList<ExpenseMonthlySeriesDto> MonthlyExpenseChart { get; init; }
    public required IReadOnlyList<ExpenseMonthlySeriesDto> MonthlyIncomeChart { get; init; }
}
