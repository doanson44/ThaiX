using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.ExpenseTracker.Commands.CreateTransactionTag;

/// <summary>
/// Command to link a tag to a transaction.
/// </summary>
[InvalidateCache(CacheGroups.TransactionTags)]
public sealed record CreateTransactionTagCommand : IAppCommand<Guid>
{
    public required Guid TransactionId { get; init; }
    public required Guid TagId { get; init; }
}
