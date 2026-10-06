using FluentAssertions;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Application.UnitTests.Domain.Aggregates;

public sealed class ExpenseTrackerTests
{
    [Fact]
    public void Wallet_ApplyIncomeAndExpense_ShouldMaintainCurrentBalance()
    {
        // Arrange
        var wallet = Wallet.Create("Cash", WalletType.Cash, "USD", 100m);

        // Act
        wallet.ApplyIncome(25m);
        wallet.ApplyExpense(10m);

        // Assert
        wallet.CurrentBalance.Should().Be(115m);
    }

    [Fact]
    public void Transfer_Create_WithSameSourceAndTarget_ShouldThrow()
    {
        // Arrange
        var walletId = Guid.NewGuid();

        // Act
        var act = () => Transfer.Create(walletId, walletId, 10m, DateTime.UtcNow, null);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void RecurringTransaction_Create_WithInvalidEndDate_ShouldThrow()
    {
        // Arrange
        var nextRun = DateTime.UtcNow;

        // Act
        var act = () => RecurringTransaction.Create(
            Guid.NewGuid(),
            null,
            ExpenseTransactionType.Expense,
            10m,
            RecurringFrequency.Monthly,
            nextRun,
            nextRun,
            null);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void SavingGoal_Create_WhenCurrentAmountExceedsTarget_ShouldThrow()
    {
        // Arrange
        // Act
        var act = () => SavingGoal.Create(Guid.NewGuid(), "Goal", 100m, 101m, null);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
