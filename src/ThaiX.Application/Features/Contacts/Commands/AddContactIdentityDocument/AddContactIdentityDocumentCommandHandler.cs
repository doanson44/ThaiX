using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactIdentityDocument;

/// <summary>
/// Adds the contact's identity document by mutating ContactIdentityDocuments directly.
/// </summary>
public sealed class AddContactIdentityDocumentCommandHandler : IRequestHandler<AddContactIdentityDocumentCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IEncryptionService _encryptionService;

    public AddContactIdentityDocumentCommandHandler(
        IApplicationDbContext dbContext,
        IEncryptionService encryptionService)
    {
        _dbContext = dbContext;
        _encryptionService = encryptionService;
    }

    public async Task<Guid> Handle(AddContactIdentityDocumentCommand request, CancellationToken cancellationToken)
    {
        var contactExists = await _dbContext.Contacts
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.ContactId, cancellationToken);
        if (!contactExists)
            throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.ContactId}' not found.");

        var identityDocumentExists = await _dbContext.ContactIdentityDocuments
            .AsNoTracking()
            .AnyAsync(d => d.ContactId == request.ContactId, cancellationToken);
        if (identityDocumentExists)
            throw new OperationFailedException(ErrorCodes.INVALID_REQUEST, "Contact already has an identity document.");

        var encryptedDocumentNumber = _encryptionService.Encrypt(request.DocumentNumber);
        var last4 = request.DocumentNumber.Length >= 4
            ? request.DocumentNumber[^4..]
            : request.DocumentNumber;

        var identityDocument = ContactIdentityDocument.Create(
            request.ContactId,
            request.DocumentType,
            encryptedDocumentNumber,
            last4,
            request.IssuedBy,
            request.IssuedPlace,
            request.IssuedDate,
            request.ExpiryDate);

        _dbContext.ContactIdentityDocuments.Add(identityDocument);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return identityDocument.Id;
    }
}
