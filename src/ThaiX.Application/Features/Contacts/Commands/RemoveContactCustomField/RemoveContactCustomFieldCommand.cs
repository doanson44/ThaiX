using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactCustomField;

[InvalidateCache(CacheGroups.Contacts)]
public sealed record RemoveContactCustomFieldCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required string Key { get; init; }
}
