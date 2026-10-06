using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactAddress;

[InvalidateCache(CacheGroups.Contacts)]
public sealed record RemoveContactAddressCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required Guid AddressId { get; init; }
}
