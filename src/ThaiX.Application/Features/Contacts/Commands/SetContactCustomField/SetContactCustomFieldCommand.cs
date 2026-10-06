using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.SetContactCustomField;

[InvalidateCache(CacheGroups.Contacts)]
public sealed record SetContactCustomFieldCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required string Key { get; init; }
    public required string Value { get; init; }
}
