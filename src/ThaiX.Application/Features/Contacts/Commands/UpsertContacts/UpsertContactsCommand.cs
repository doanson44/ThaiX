using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.Contacts.Models;

namespace ThaiX.Application.Features.Contacts.Commands.UpsertContacts;

/// <summary>
/// Upserts a batch of contact import rows: find by normalized phone (primary) or email, then update or create via aggregate.
/// </summary>
[InvalidateCache(CacheGroups.Contacts)]
public sealed record UpsertContactsCommand : IAppCommand<UpsertContactsResult>
{
    public required IReadOnlyCollection<ParsedImportRow> Rows { get; init; }
}
