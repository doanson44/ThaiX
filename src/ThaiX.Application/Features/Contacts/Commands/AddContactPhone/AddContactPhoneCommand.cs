using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactPhone;

[InvalidateCache(CacheGroups.Contacts)]
public sealed record AddContactPhoneCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required string Value { get; init; }
    public bool IsPrimary { get; init; }
}
