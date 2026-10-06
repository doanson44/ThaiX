using ThaiX.Domain.Aggregates.ExpenseTracker;
namespace ThaiX.Application.Features.ExpenseTracker.Models;

/// <summary>
/// DTO for wallet.
/// </summary>
public sealed record ExpenseWalletDto
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public required WalletType WalletType { get; init; }
    public required string Currency { get; init; }
    public required decimal CurrentBalance { get; init; }
}
