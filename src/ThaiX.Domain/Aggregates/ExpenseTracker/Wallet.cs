using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.ExpenseTracker;

public sealed class Wallet : BaseAuditableEntity
{
    private Wallet()
    {
    }

    public string Name { get; private set; } = null!;
    public WalletType WalletType { get; private set; }
    public string Currency { get; private set; } = null!;
    public decimal CurrentBalance { get; private set; }

    public static Wallet Create(string name, WalletType walletType, string currency, decimal initialBalance)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        return new Wallet
        {
            Id = Guid.NewGuid(),
            Name = name.Trim(),
            WalletType = walletType,
            Currency = currency.Trim().ToUpperInvariant(),
            CurrentBalance = initialBalance
        };
    }

    public void Update(string name, WalletType walletType, string currency)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(currency);

        Name = name.Trim();
        WalletType = walletType;
        Currency = currency.Trim().ToUpperInvariant();
    }

    public void ApplyIncome(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        CurrentBalance += amount;
    }

    public void ApplyExpense(decimal amount)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(amount);
        CurrentBalance -= amount;
    }

    public void SoftDelete()
    {
        Delete();
    }
}
