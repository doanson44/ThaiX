using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactEmail;

/// <summary>
/// Command to remove an email address from a contact.
/// </summary>
[InvalidateCache(CacheGroups.Contacts)]
public sealed record RemoveContactEmailCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required Guid EmailId { get; init; }
}
