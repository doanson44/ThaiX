using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.ExpenseTracker;

public sealed class Transaction : BaseAuditableEntity
{
    private Transaction()
    {
    }

    public Guid WalletId { get; private set; }
    public Wallet Wallet { get; private set; } = null!;
    public Guid? CategoryId { get; private set; }
    public Category? Category { get; private set; }
    public ExpenseTransactionType TransactionType { get; private set; }
    public decimal Amount { get; private set; }
    public DateTime OccurredOn { get; private set; }
    public string? Note { get; private set; }

    public static Transaction Create(Guid walletId, Guid? categoryId, ExpenseTransactionType transactionType, decimal amount, DateTime occurredOn, string? note)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        return new Transaction
        {
            Id = Guid.NewGuid(),
            WalletId = walletId,
            CategoryId = categoryId,
            TransactionType = transactionType,
            Amount = amount,
            OccurredOn = occurredOn,
            Note = note?.Trim()
        };
    }

    public void Update(Guid walletId, Guid? categoryId, ExpenseTransactionType transactionType, decimal amount, DateTime occurredOn, string? note)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);

        WalletId = walletId;
        CategoryId = categoryId;
        TransactionType = transactionType;
        Amount = amount;
        OccurredOn = occurredOn;
        Note = note?.Trim();
    }

    public void SoftDelete()
    {
        Delete();
    }
}
