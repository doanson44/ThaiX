using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.UploadContactAvatar;

/// <summary>
/// Uploads an avatar image for a contact. Replaces existing avatar. Returns the new avatar URL.
/// </summary>
[InvalidateCache(CacheGroups.Contacts)]
[InvalidateCache(CacheGroups.UserProfiles)]
public sealed record UploadContactAvatarCommand : IAppCommand<string>
{
    public Guid ContactId { get; init; }
    public required Stream FileStream { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
}
