using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.SetContactPhonePrimary;

/// <summary>
/// Command to set an existing contact phone as the primary phone.
/// </summary>
[InvalidateCache(CacheGroups.Contacts)]
public sealed record SetContactPhonePrimaryCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required Guid PhoneId { get; init; }
}
