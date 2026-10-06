using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateExpenseTag;

/// <summary>
/// Command to create an expense tag.
/// </summary>
[InvalidateCache(CacheGroups.ExpenseTags)]
public sealed record CreateExpenseTagCommand : IAppCommand<Guid>
{
    public required string Name { get; init; }
    public string? ColorHex { get; init; }
}
