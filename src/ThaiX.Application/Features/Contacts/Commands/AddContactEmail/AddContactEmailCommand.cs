using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactEmail;

/// <summary>
/// Command to add an email address to a contact.
/// </summary>
[InvalidateCache(CacheGroups.Contacts)]
public sealed record AddContactEmailCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required string Value { get; init; }
    public bool IsPrimary { get; init; }
}
