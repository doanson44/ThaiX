using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateSavingGoal;

/// <summary>
/// Command to create a saving goal.
/// </summary>
[InvalidateCache(CacheGroups.SavingGoals)]
public sealed record CreateSavingGoalCommand : IAppCommand<Guid>
{
    public required Guid WalletId { get; init; }
    public required string Name { get; init; }
    public required decimal TargetAmount { get; init; }
    public decimal CurrentAmount { get; init; }
    public DateTime? TargetDate { get; init; }
}
