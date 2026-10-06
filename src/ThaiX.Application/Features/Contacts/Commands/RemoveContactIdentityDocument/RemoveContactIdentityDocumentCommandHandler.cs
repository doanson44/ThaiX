using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactIdentityDocument;

/// <summary>
/// Removes the contact's identity document by deleting from ContactIdentityDocuments directly.
/// </summary>
public sealed class RemoveContactIdentityDocumentCommandHandler : IRequestHandler<RemoveContactIdentityDocumentCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public RemoveContactIdentityDocumentCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(RemoveContactIdentityDocumentCommand request, CancellationToken cancellationToken)
    {
        var contactExists = await _dbContext.Contacts
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.ContactId, cancellationToken);
        if (!contactExists)
            throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.ContactId}' not found.");

        var rowsDeleted = await _dbContext.ContactIdentityDocuments
            .Where(d => d.Id == request.IdentityDocumentId && d.ContactId == request.ContactId)
            .ExecuteDeleteAsync(cancellationToken);

        if (rowsDeleted == 0)
            throw new OperationFailedException(ErrorCodes.CONTACT_ITEM_NOT_FOUND, "Identity document not found for this contact.");

        await _dbContext.SaveChangesAsync(cancellationToken);

        return request.ContactId;
    }
}
