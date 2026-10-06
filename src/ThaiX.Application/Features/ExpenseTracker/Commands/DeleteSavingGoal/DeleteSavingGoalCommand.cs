using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteSavingGoal;

/// <summary>
/// Command to soft-delete a saving goal.
/// </summary>
[InvalidateCache(CacheGroups.SavingGoals)]
public sealed record DeleteSavingGoalCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
