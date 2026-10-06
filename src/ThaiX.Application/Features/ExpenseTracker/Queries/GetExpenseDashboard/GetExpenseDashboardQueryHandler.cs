using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.ExpenseTracker.Models;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Application.Features.ExpenseTracker.Queries.GetExpenseDashboard;

/// <summary>
/// Handler for GetExpenseDashboardQuery.
/// </summary>
public sealed class GetExpenseDashboardQueryHandler : IRequestHandler<GetExpenseDashboardQuery, ExpenseDashboardDataDto>
{
    private readonly IApplicationDbContext _dbContext;

    public GetExpenseDashboardQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<ExpenseDashboardDataDto> Handle(GetExpenseDashboardQuery request, CancellationToken cancellationToken)
    {
        var today = DateTime.UtcNow.Date;
        var monthStart = new DateTime(today.Year, today.Month, 1);

        var totalAsset = await _dbContext.Wallets.AsNoTracking().SumAsync(x => x.CurrentBalance, cancellationToken);

        var incomeToday = await _dbContext.ExpenseTransactions.AsNoTracking()
            .Where(x => x.TransactionType == ExpenseTransactionType.Income && x.OccurredOn >= today && x.OccurredOn < today.AddDays(1))
            .SumAsync(x => x.Amount, cancellationToken);

        var expenseToday = await _dbContext.ExpenseTransactions.AsNoTracking()
            .Where(x => x.TransactionType == ExpenseTransactionType.Expense && x.OccurredOn >= today && x.OccurredOn < today.AddDays(1))
            .SumAsync(x => x.Amount, cancellationToken);

        var incomeThisMonth = await _dbContext.ExpenseTransactions.AsNoTracking()
            .Where(x => x.TransactionType == ExpenseTransactionType.Income && x.OccurredOn >= monthStart)
            .SumAsync(x => x.Amount, cancellationToken);

        var expenseThisMonth = await _dbContext.ExpenseTransactions.AsNoTracking()
            .Where(x => x.TransactionType == ExpenseTransactionType.Expense && x.OccurredOn >= monthStart)
            .SumAsync(x => x.Amount, cancellationToken);

        var expenseByCategory = await _dbContext.ExpenseTransactions.AsNoTracking()
            .Where(x => x.TransactionType == ExpenseTransactionType.Expense)
            .GroupBy(x => x.Category != null ? x.Category.Name : "Uncategorized")
            .Select(g => new ExpenseCategoryAmountDto { CategoryName = g.Key, Amount = g.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Amount)
            .Take(10)
            .ToListAsync(cancellationToken);

        var incomeByCategory = await _dbContext.ExpenseTransactions.AsNoTracking()
            .Where(x => x.TransactionType == ExpenseTransactionType.Income)
            .GroupBy(x => x.Category != null ? x.Category.Name : "Uncategorized")
            .Select(g => new ExpenseCategoryAmountDto { CategoryName = g.Key, Amount = g.Sum(x => x.Amount) })
            .OrderByDescending(x => x.Amount)
            .Take(10)
            .ToListAsync(cancellationToken);

        var recentTransactions = await _dbContext.ExpenseTransactions.AsNoTracking()
            .OrderByDescending(x => x.OccurredOn)
            .Take(20)
            .Select(x => new ExpenseTransactionDto
            {
                Id = x.Id,
                WalletId = x.WalletId,
                CategoryId = x.CategoryId,
                TransactionType = x.TransactionType,
                Amount = x.Amount,
                OccurredOn = x.OccurredOn,
                Note = x.Note
            })
            .ToListAsync(cancellationToken);

        var monthlyExpense = await _dbContext.ExpenseTransactions.AsNoTracking()
            .Where(x => x.TransactionType == ExpenseTransactionType.Expense)
            .GroupBy(x => new { x.OccurredOn.Year, x.OccurredOn.Month })
            .Select(g => new ExpenseMonthlySeriesDto { Year = g.Key.Year, Month = g.Key.Month, Amount = g.Sum(x => x.Amount) })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        var monthlyIncome = await _dbContext.ExpenseTransactions.AsNoTracking()
            .Where(x => x.TransactionType == ExpenseTransactionType.Income)
            .GroupBy(x => new { x.OccurredOn.Year, x.OccurredOn.Month })
            .Select(g => new ExpenseMonthlySeriesDto { Year = g.Key.Year, Month = g.Key.Month, Amount = g.Sum(x => x.Amount) })
            .OrderBy(x => x.Year).ThenBy(x => x.Month)
            .ToListAsync(cancellationToken);

        var summary = new ExpenseDashboardSummaryDto
        {
            TotalAsset = totalAsset,
            IncomeToday = incomeToday,
            ExpenseToday = expenseToday,
            IncomeThisMonth = incomeThisMonth,
            ExpenseThisMonth = expenseThisMonth
        };

        return new ExpenseDashboardDataDto
        {
            Summary = summary,
            CashFlow = incomeThisMonth - expenseThisMonth,
            ExpenseByCategory = expenseByCategory,
            IncomeByCategory = incomeByCategory,
            RecentTransactions = recentTransactions,
            MonthlyExpenseChart = monthlyExpense,
            MonthlyIncomeChart = monthlyIncome
        };
    }
}
