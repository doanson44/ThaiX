namespace ThaiX.Client.Models.ExpenseTracker;

public sealed class ExpenseWalletDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string WalletType { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
    public decimal CurrentBalance { get; init; }
}

public sealed class ExpenseCategoryDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string CategoryType { get; init; } = string.Empty;
    public Guid? ParentCategoryId { get; init; }
}

public sealed class ExpenseTransactionDto
{
    public Guid Id { get; init; }
    public Guid WalletId { get; init; }
    public Guid? CategoryId { get; init; }
    public string TransactionType { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime OccurredOn { get; init; }
    public string? Note { get; init; }
}

public sealed class ExpenseTransferDto
{
    public Guid Id { get; init; }
    public Guid SourceWalletId { get; init; }
    public Guid TargetWalletId { get; init; }
    public decimal Amount { get; init; }
    public DateTime TransferredOn { get; init; }
    public string? Note { get; init; }
}

public sealed class ExpenseBudgetDto
{
    public Guid Id { get; init; }
    public Guid CategoryId { get; init; }
    public string Period { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
}

public sealed class ExpenseSavingGoalDto
{
    public Guid Id { get; init; }
    public Guid WalletId { get; init; }
    public string Name { get; init; } = string.Empty;
    public decimal TargetAmount { get; init; }
    public decimal CurrentAmount { get; init; }
    public DateTime? TargetDate { get; init; }
}

public sealed class ExpenseTagDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? ColorHex { get; init; }
}

public sealed class ExpenseTransactionTagDto
{
    public Guid Id { get; init; }
    public Guid TransactionId { get; init; }
    public Guid TagId { get; init; }
}

public sealed class ExpenseRecurringTransactionDto
{
    public Guid Id { get; init; }
    public Guid WalletId { get; init; }
    public Guid? CategoryId { get; init; }
    public string TransactionType { get; init; } = string.Empty;
    public decimal Amount { get; init; }
    public string Frequency { get; init; } = string.Empty;
    public DateTime NextRun { get; init; }
    public DateTime? EndDate { get; init; }
    public bool IsActive { get; init; }
    public string? Note { get; init; }
}

public sealed class ExpenseCategoryAmountDto
{
    public string CategoryName { get; init; } = string.Empty;
    public decimal Amount { get; init; }
}

public sealed class ExpenseMonthlySeriesDto
{
    public int Year { get; init; }
    public int Month { get; init; }
    public decimal Amount { get; init; }
}

public sealed class ExpenseDashboardSummaryDto
{
    public decimal TotalAsset { get; init; }
    public decimal IncomeToday { get; init; }
    public decimal ExpenseToday { get; init; }
    public decimal IncomeThisMonth { get; init; }
    public decimal ExpenseThisMonth { get; init; }
}

public sealed class ExpenseDashboardDataDto
{
    public ExpenseDashboardSummaryDto Summary { get; init; } = new();
    public decimal CashFlow { get; init; }
    public IReadOnlyList<ExpenseCategoryAmountDto> ExpenseByCategory { get; init; } = [];
    public IReadOnlyList<ExpenseCategoryAmountDto> IncomeByCategory { get; init; } = [];
    public IReadOnlyList<ExpenseTransactionDto> RecentTransactions { get; init; } = [];
    public IReadOnlyList<ExpenseMonthlySeriesDto> MonthlyExpenseChart { get; init; } = [];
    public IReadOnlyList<ExpenseMonthlySeriesDto> MonthlyIncomeChart { get; init; } = [];
}

public sealed class ExpenseListRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
    public string? SearchTerm { get; set; }
}

public enum ExpenseResourceKind
{
    Wallet,
    Category,
    Transaction,
    Transfer,
    Budget,
    SavingGoal,
    Tag,
    TransactionTag,
    Recurring
}

public sealed class ExpenseWalletWriteRequest
{
    public string Name { get; set; } = string.Empty;
    public string WalletType { get; set; } = "Cash";
    public string Currency { get; set; } = "VND";
    public decimal InitialBalance { get; set; }
}

public sealed class ExpenseCategoryWriteRequest
{
    public string Name { get; set; } = string.Empty;
    public string CategoryType { get; set; } = "Expense";
    public Guid? ParentCategoryId { get; set; }
}

public sealed class ExpenseTransactionWriteRequest
{
    public Guid WalletId { get; set; }
    public Guid? CategoryId { get; set; }
    public string TransactionType { get; set; } = "Expense";
    public decimal Amount { get; set; }
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }
}

public sealed class ExpenseTransferWriteRequest
{
    public Guid SourceWalletId { get; set; }
    public Guid TargetWalletId { get; set; }
    public decimal Amount { get; set; }
    public DateTime TransferredOn { get; set; } = DateTime.UtcNow;
    public string? Note { get; set; }
}

public sealed class ExpenseBudgetWriteRequest
{
    public Guid CategoryId { get; set; }
    public string Period { get; set; } = "Monthly";
    public decimal Amount { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.Date.AddMonths(1).AddDays(-1);
}

public sealed class ExpenseSavingGoalWriteRequest
{
    public Guid WalletId { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public DateTime? TargetDate { get; set; }
}

public sealed class ExpenseTagWriteRequest
{
    public string Name { get; set; } = string.Empty;
    public string? ColorHex { get; set; }
}

public sealed class ExpenseTransactionTagWriteRequest
{
    public Guid TransactionId { get; set; }
    public Guid TagId { get; set; }
}

public sealed class ExpenseRecurringWriteRequest
{
    public Guid WalletId { get; set; }
    public Guid? CategoryId { get; set; }
    public string TransactionType { get; set; } = "Expense";
    public decimal Amount { get; set; }
    public string Frequency { get; set; } = "Monthly";
    public DateTime NextRun { get; set; } = DateTime.UtcNow.Date.AddDays(1);
    public DateTime? EndDate { get; set; }
    public bool IsActive { get; set; } = true;
    public string? Note { get; set; }
}

/// <summary>
/// Shared dialog bag for Expense Tracker create/edit (one dialog, many kinds).
/// </summary>
public sealed class ExpenseTrackerFormState
{
    public ExpenseResourceKind Kind { get; set; }
    public bool IsEdit { get; set; }
    public Guid EntityId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string Currency { get; set; } = "VND";
    public decimal InitialBalance { get; set; }
    public decimal Amount { get; set; }
    public decimal TargetAmount { get; set; }
    public decimal CurrentAmount { get; set; }
    public Guid WalletId { get; set; }
    public Guid? CategoryId { get; set; }
    public Guid SourceWalletId { get; set; }
    public Guid TargetWalletId { get; set; }
    public Guid TransactionId { get; set; }
    public Guid TagId { get; set; }
    public DateTime OccurredOn { get; set; } = DateTime.UtcNow;
    public DateTime TransferredOn { get; set; } = DateTime.UtcNow;
    public DateTime StartDate { get; set; } = DateTime.UtcNow.Date;
    public DateTime EndDate { get; set; } = DateTime.UtcNow.Date.AddMonths(1).AddDays(-1);
    public DateTime NextRun { get; set; } = DateTime.UtcNow.Date.AddDays(1);
    public DateTime? TargetDate { get; set; }
    public DateTime? RecurringEndDate { get; set; }
    public string Period { get; set; } = "Monthly";
    public string Frequency { get; set; } = "Monthly";
    public string? Note { get; set; }
    public string? ColorHex { get; set; }
    public bool IsActive { get; set; } = true;
    public Guid? ParentCategoryId { get; set; }
}
