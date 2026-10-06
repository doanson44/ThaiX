using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.SetContactEmailPrimary;

/// <summary>
/// Command to set an existing contact email as the primary email.
/// </summary>
[InvalidateCache(CacheGroups.Contacts)]
public sealed record SetContactEmailPrimaryCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required Guid EmailId { get; init; }
}
