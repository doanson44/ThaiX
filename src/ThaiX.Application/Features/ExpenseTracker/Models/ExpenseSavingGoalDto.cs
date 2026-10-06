namespace ThaiX.Application.Features.ExpenseTracker.Models;

/// <summary>
/// DTO for saving goal.
/// </summary>
public sealed record ExpenseSavingGoalDto
{
    public required Guid Id { get; init; }
    public required Guid WalletId { get; init; }
    public required string Name { get; init; }
    public required decimal TargetAmount { get; init; }
    public required decimal CurrentAmount { get; init; }
    public DateTime? TargetDate { get; init; }
}
