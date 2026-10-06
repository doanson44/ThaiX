using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactIdentityDocument;

[InvalidateCache(CacheGroups.Contacts)]
public sealed record RemoveContactIdentityDocumentCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required Guid IdentityDocumentId { get; init; }
}
