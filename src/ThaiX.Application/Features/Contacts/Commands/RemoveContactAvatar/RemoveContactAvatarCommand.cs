using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactAvatar;

/// <summary>
/// Removes the contact's avatar (deletes file from storage and clears AvatarUrl).
/// </summary>
[InvalidateCache(CacheGroups.Contacts)]
[InvalidateCache(CacheGroups.UserProfiles)]
public sealed record RemoveContactAvatarCommand : IAppCommand<Guid>
{
    public Guid ContactId { get; init; }
}
