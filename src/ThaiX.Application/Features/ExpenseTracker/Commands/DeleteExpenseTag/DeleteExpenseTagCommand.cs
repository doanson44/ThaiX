using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteExpenseTag;

/// <summary>
/// Command to soft-delete an expense tag.
/// </summary>
[InvalidateCache(CacheGroups.ExpenseTags)]
public sealed record DeleteExpenseTagCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
