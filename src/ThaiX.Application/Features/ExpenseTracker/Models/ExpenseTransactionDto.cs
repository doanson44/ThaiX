using ThaiX.Domain.Aggregates.ExpenseTracker;
namespace ThaiX.Application.Features.ExpenseTracker.Models;

/// <summary>
/// DTO for expense transaction.
/// </summary>
public sealed record ExpenseTransactionDto
{
    public required Guid Id { get; init; }
    public required Guid WalletId { get; init; }
    public Guid? CategoryId { get; init; }
    public required ExpenseTransactionType TransactionType { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime OccurredOn { get; init; }
    public string? Note { get; init; }
}
