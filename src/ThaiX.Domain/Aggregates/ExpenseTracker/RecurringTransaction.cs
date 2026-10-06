using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.ExpenseTracker;

public sealed class RecurringTransaction : BaseAuditableEntity
{
    private RecurringTransaction()
    {
    }

    public Guid WalletId { get; private set; }
    public Wallet Wallet { get; private set; } = null!;
    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public ExpenseTransactionType TransactionType { get; private set; }
    public decimal Amount { get; private set; }
    public RecurringFrequency Frequency { get; private set; }
    public DateTime NextRun { get; private set; }
    public DateTime? EndDate { get; private set; }
    public bool IsActive { get; private set; }
    public string? Note { get; private set; }

    public static RecurringTransaction Create(
        Guid walletId,
        Guid? categoryId,
        ExpenseTransactionType transactionType,
        decimal amount,
        RecurringFrequency frequency,
        DateTime nextRun,
        DateTime? endDate,
        string? note)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        if (endDate.HasValue && endDate.Value <= nextRun)
            throw new InvalidOperationException("Recurring transaction end date must be greater than next run date.");

        return new RecurringTransaction
        {
            Id = Guid.NewGuid(),
            WalletId = walletId,
            CategoryId = categoryId,
            TransactionType = transactionType,
            Amount = amount,
            Frequency = frequency,
            NextRun = nextRun,
            EndDate = endDate,
            IsActive = true,
            Note = note?.Trim()
        };
    }

    public void Update(
        Guid walletId,
        Guid? categoryId,
        ExpenseTransactionType transactionType,
        decimal amount,
        RecurringFrequency frequency,
        DateTime nextRun,
        DateTime? endDate,
        bool isActive,
        string? note)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        if (endDate.HasValue && endDate.Value <= nextRun)
            throw new InvalidOperationException("Recurring transaction end date must be greater than next run date.");

        WalletId = walletId;
        CategoryId = categoryId;
        TransactionType = transactionType;
        Amount = amount;
        Frequency = frequency;
        NextRun = nextRun;
        EndDate = endDate;
        IsActive = isActive;
        Note = note?.Trim();
    }

    public void SoftDelete()
    {
        Delete();
    }
}