using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactTag;

[InvalidateCache(CacheGroups.Contacts)]
public sealed record RemoveContactTagCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required Guid TagId { get; init; }
}
