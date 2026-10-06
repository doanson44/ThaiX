using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.ExpenseTracker;

public sealed class SavingGoal : BaseAuditableEntity
{
    private SavingGoal()
    {
    }

    public Guid WalletId { get; private set; }
    public Wallet Wallet { get; private set; } = null!;
    public string Name { get; private set; } = null!;
    public decimal TargetAmount { get; private set; }
    public decimal CurrentAmount { get; private set; }
    public DateTime? TargetDate { get; private set; }

    public static SavingGoal Create(Guid walletId, string name, decimal targetAmount, decimal currentAmount, DateTime? targetDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetAmount);
        if (currentAmount < 0 || currentAmount > targetAmount)
            throw new InvalidOperationException("Saving goal current amount must be between 0 and target amount.");

        return new SavingGoal
        {
            Id = Guid.NewGuid(),
            WalletId = walletId,
            Name = name.Trim(),
            TargetAmount = targetAmount,
            CurrentAmount = currentAmount,
            TargetDate = targetDate
        };
    }

    public void Update(Guid walletId, string name, decimal targetAmount, decimal currentAmount, DateTime? targetDate)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(targetAmount);
        if (currentAmount < 0 || currentAmount > targetAmount)
            throw new InvalidOperationException("Saving goal current amount must be between 0 and target amount.");

        WalletId = walletId;
        Name = name.Trim();
        TargetAmount = targetAmount;
        CurrentAmount = currentAmount;
        TargetDate = targetDate;
    }

    public void SoftDelete()
    {
        Delete();
    }
}
