using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactPhone;

[InvalidateCache(CacheGroups.Contacts)]
public sealed record RemoveContactPhoneCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required Guid PhoneId { get; init; }
}
