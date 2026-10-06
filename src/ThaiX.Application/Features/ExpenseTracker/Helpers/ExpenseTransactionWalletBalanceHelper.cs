using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Application.Features.ExpenseTracker.Helpers;

/// <summary>
/// Applies income/expense amounts to a wallet balance.
/// </summary>
internal static class ExpenseTransactionWalletBalanceHelper
{
    public static void ApplyWallet(Wallet wallet, ExpenseTransactionType transactionType, decimal amount, bool reverse)
    {
        if (reverse)
        {
            if (transactionType == ExpenseTransactionType.Income)
                wallet.ApplyExpense(amount);
            else
                wallet.ApplyIncome(amount);

            return;
        }

        if (transactionType == ExpenseTransactionType.Income)
            wallet.ApplyIncome(amount);
        else
            wallet.ApplyExpense(amount);
    }
}
