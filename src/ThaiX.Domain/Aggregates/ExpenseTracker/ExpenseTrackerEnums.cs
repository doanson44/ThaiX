namespace ThaiX.Domain.Aggregates.ExpenseTracker;

public enum WalletType
{
    Cash = 1,
    Bank = 2,
    EWallet = 3,
    Crypto = 4,
    Other = 5
}

public enum CategoryType
{
    Income = 1,
    Expense = 2
}

public enum ExpenseTransactionType
{
    Income = 1,
    Expense = 2
}

public enum BudgetPeriod
{
    Monthly = 1,
    Quarterly = 2,
    Yearly = 3
}

public enum RecurringFrequency
{
    Daily = 1,
    Weekly = 2,
    Monthly = 3,
    Yearly = 4
}