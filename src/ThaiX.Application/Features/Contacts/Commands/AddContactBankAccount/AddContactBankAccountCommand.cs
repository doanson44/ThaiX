using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactBankAccount;

[InvalidateCache(CacheGroups.Contacts)]
public sealed record AddContactBankAccountCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required string BankCode { get; init; }
    public string? BranchName { get; init; }

    /// <summary>
    /// Plain-text account number. Will be encrypted before storage.
    /// </summary>
    public required string AccountNumber { get; init; }

    public required string AccountName { get; init; }
    public required string CurrencyCode { get; init; }
    public bool IsPrimary { get; init; }
}
