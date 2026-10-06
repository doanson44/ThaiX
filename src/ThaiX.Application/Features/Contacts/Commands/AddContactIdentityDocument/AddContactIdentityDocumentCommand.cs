using ThaiX.Application.Common.Caching;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactIdentityDocument;

/// <summary>
/// Sets the contact's single identity document. Fails if contact already has one.
/// </summary>
[InvalidateCache(CacheGroups.Contacts)]
public sealed record AddContactIdentityDocumentCommand : IAppCommand<Guid>
{
    public required Guid ContactId { get; init; }
    public required string DocumentType { get; init; }

    /// <summary>
    /// Plain-text document number. Will be encrypted before storage.
    /// </summary>
    public required string DocumentNumber { get; init; }

    public required string IssuedBy { get; init; }
    public required string IssuedPlace { get; init; }
    public required DateOnly IssuedDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }
}
