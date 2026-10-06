using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.ExpenseTracker;

public sealed class Transfer : BaseAuditableEntity
{
    private Transfer()
    {
    }

    public Guid SourceWalletId { get; private set; }
    public Wallet SourceWallet { get; private set; } = null!;
    public Guid TargetWalletId { get; private set; }
    public Wallet TargetWallet { get; private set; } = null!;
    public decimal Amount { get; private set; }
    public DateTime TransferredOn { get; private set; }
    public string? Note { get; private set; }

    public static Transfer Create(Guid sourceWalletId, Guid targetWalletId, decimal amount, DateTime transferredOn, string? note)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        if (sourceWalletId == targetWalletId)
            throw new InvalidOperationException("Transfer source and target wallets must be different.");

        return new Transfer
        {
            Id = Guid.NewGuid(),
            SourceWalletId = sourceWalletId,
            TargetWalletId = targetWalletId,
            Amount = amount,
            TransferredOn = transferredOn,
            Note = note?.Trim()
        };
    }

    public void Update(Guid sourceWalletId, Guid targetWalletId, decimal amount, DateTime transferredOn, string? note)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        if (sourceWalletId == targetWalletId)
            throw new InvalidOperationException("Transfer source and target wallets must be different.");

        SourceWalletId = sourceWalletId;
        TargetWalletId = targetWalletId;
        Amount = amount;
        TransferredOn = transferredOn;
        Note = note?.Trim();
    }

    public void SoftDelete()
    {
        Delete();
    }
}
