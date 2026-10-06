using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.UpdateTransactionTag;

/// <summary>
/// Command to update a transaction-tag link.
/// </summary>
[InvalidateCache(CacheGroups.TransactionTags)]
public sealed record UpdateTransactionTagCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
    public required Guid TransactionId { get; init; }
    public required Guid TagId { get; init; }
}
