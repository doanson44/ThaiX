namespace ThaiX.Application.Features.ExpenseTracker.Models;

/// <summary>
/// DTO for transaction-tag link.
/// </summary>
public sealed record ExpenseTransactionTagDto
{
    public required Guid Id { get; init; }
    public required Guid TransactionId { get; init; }
    public required Guid TagId { get; init; }
}
