using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactTag;

[InvalidateCache(CacheGroups.Contacts)]
public sealed record AddContactTagCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required string Name { get; init; }
}
