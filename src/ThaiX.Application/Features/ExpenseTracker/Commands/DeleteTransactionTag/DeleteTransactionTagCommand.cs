using MediatR;
using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.DeleteTransactionTag;

/// <summary>
/// Command to soft-delete a transaction-tag link.
/// </summary>
[InvalidateCache(CacheGroups.TransactionTags)]
public sealed record DeleteTransactionTagCommand : IAppCommand<Unit>
{
    public required Guid Id { get; init; }
}
