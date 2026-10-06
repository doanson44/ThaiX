using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactBankAccount;

[InvalidateCache(CacheGroups.Contacts)]
public sealed record RemoveContactBankAccountCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required Guid BankAccountId { get; init; }
}
