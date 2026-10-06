using ThaiX.Domain.Aggregates.Portfolios;
using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.AssetPositions;

/// <summary>
/// A bank saving or deposit account position.
/// Tracks principal, interest rate, maturity, and lifecycle.
/// Unlike trading positions, saving positions do not use weighted average cost.
/// </summary>
public sealed class SavingPosition : BaseAuditableEntity
{
    private SavingPosition()
    {
    }

    /// <summary>The portfolio this position belongs to.</summary>
    public Guid PortfolioId { get; private set; }

    public Portfolio Portfolio { get; private set; } = null!;

    /// <summary>Name of the bank or financial institution.</summary>
    public string BankName { get; private set; } = null!;

    /// <summary>Optional account number or deposit certificate number.</summary>
    public string? AccountNumber { get; private set; }

    /// <summary>Deposited principal amount.</summary>
    public decimal PrincipalAmount { get; private set; }

    /// <summary>Annual interest rate as a percentage (e.g. 7.5 for 7.5%).</summary>
    public decimal InterestRate { get; private set; }

    /// <summary>How interest is compounded.</summary>
    public InterestType InterestType { get; private set; }

    /// <summary>Date the deposit was made.</summary>
    public DateOnly DepositDate { get; private set; }

    /// <summary>Date the deposit matures. Null for demand/on-call deposits.</summary>
    public DateOnly? MaturityDate { get; private set; }

    /// <summary>Current lifecycle status.</summary>
    public SavingStatus Status { get; private set; }

    /// <summary>User-defined note or memo.</summary>
    public string? Note { get; private set; }

    public static SavingPosition Create(
        Guid portfolioId,
        string bankName,
        string? accountNumber,
        decimal principalAmount,
        decimal interestRate,
        InterestType interestType,
        DateOnly depositDate,
        DateOnly? maturityDate,
        string? note)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bankName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(principalAmount);
        ArgumentOutOfRangeException.ThrowIfNegative(interestRate);

        return new SavingPosition
        {
            Id = Guid.NewGuid(),
            PortfolioId = portfolioId,
            BankName = bankName.Trim(),
            AccountNumber = accountNumber?.Trim(),
            PrincipalAmount = principalAmount,
            InterestRate = interestRate,
            InterestType = interestType,
            DepositDate = depositDate,
            MaturityDate = maturityDate,
            Status = SavingStatus.Active,
            Note = note?.Trim()
        };
    }

    public void Update(
        string bankName,
        string? accountNumber,
        decimal principalAmount,
        decimal interestRate,
        InterestType interestType,
        DateOnly depositDate,
        DateOnly? maturityDate,
        string? note)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(bankName);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(principalAmount);
        ArgumentOutOfRangeException.ThrowIfNegative(interestRate);

        BankName = bankName.Trim();
        AccountNumber = accountNumber?.Trim();
        PrincipalAmount = principalAmount;
        InterestRate = interestRate;
        InterestType = interestType;
        DepositDate = depositDate;
        MaturityDate = maturityDate;
        Note = note?.Trim();
    }

    /// <summary>Marks the deposit as matured.</summary>
    public void MarkMatured()
    {
        if (Status == SavingStatus.Active)
            Status = SavingStatus.Matured;
    }

    /// <summary>Records a withdrawal. Uses early withdrawal if before maturity.</summary>
    public void Withdraw(DateOnly withdrawalDate)
    {
        if (Status is SavingStatus.Withdrawn or SavingStatus.EarlyWithdrawn)
            return;

        Status = MaturityDate.HasValue && withdrawalDate < MaturityDate.Value
            ? SavingStatus.EarlyWithdrawn
            : SavingStatus.Withdrawn;
    }

    public void SoftDelete()
    {
        Delete();
    }
}
