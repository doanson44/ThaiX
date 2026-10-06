using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactSocialLink;

[InvalidateCache(CacheGroups.Contacts)]
public sealed record AddContactSocialLinkCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required string Platform { get; init; }
    public required string Url { get; init; }
}
