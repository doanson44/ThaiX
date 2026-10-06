using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateExpenseTag;

/// <summary>
/// Command to update an expense tag.
/// </summary>
[InvalidateCache(CacheGroups.ExpenseTags)]
public sealed record UpdateExpenseTagCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public required string Name { get; init; }
    public string? ColorHex { get; init; }
}
