using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateSavingGoal;

/// <summary>
/// Command to update a saving goal.
/// </summary>
[InvalidateCache(CacheGroups.SavingGoals)]
public sealed record UpdateSavingGoalCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public required Guid WalletId { get; init; }
    public required string Name { get; init; }
    public required decimal TargetAmount { get; init; }
    public decimal CurrentAmount { get; init; }
    public DateTime? TargetDate { get; init; }
}
