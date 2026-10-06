namespace ThaiX.Application.Features.ExpenseTracker.Models;

/// <summary>
/// DTO for wallet transfer.
/// </summary>
public sealed record ExpenseTransferDto
{
    public required Guid Id { get; init; }
    public required Guid SourceWalletId { get; init; }
    public required Guid TargetWalletId { get; init; }
    public required decimal Amount { get; init; }
    public required DateTime TransferredOn { get; init; }
    public string? Note { get; init; }
}
