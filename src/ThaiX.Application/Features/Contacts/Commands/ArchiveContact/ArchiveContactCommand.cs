using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.ArchiveContact;

/// <summary>
/// Command to archive or unarchive a contact.
/// </summary>
[InvalidateCache(CacheGroups.Contacts)]
public sealed record ArchiveContactCommand : IAppCommand<Guid>
{
    public required Guid Id { get; init; }

    /// <summary>
    /// true to archive, false to unarchive.
    /// </summary>
    public required bool Archive { get; init; }
}
