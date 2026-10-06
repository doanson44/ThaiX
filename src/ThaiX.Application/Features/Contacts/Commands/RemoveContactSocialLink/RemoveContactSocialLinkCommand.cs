using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactSocialLink;

[InvalidateCache(CacheGroups.Contacts)]
public sealed record RemoveContactSocialLinkCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required Guid SocialLinkId { get; init; }
}
