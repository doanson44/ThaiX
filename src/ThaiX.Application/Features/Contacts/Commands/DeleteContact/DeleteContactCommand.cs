using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.DeleteContact;

/// <summary>
/// Command to soft-delete a contact.
/// </summary>
[InvalidateCache(CacheGroups.Contacts)]
public sealed record DeleteContactCommand : IAppCommand<Guid>
{
    public required Guid Id { get; init; }
}
