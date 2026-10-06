using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactTag;

/// <summary>
/// Queries/updates ContactTags directly without loading the Contact aggregate.
/// </summary>
public sealed class AddContactTagCommandHandler : IRequestHandler<AddContactTagCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public AddContactTagCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(AddContactTagCommand request, CancellationToken cancellationToken)
    {
        var contactExists = await _dbContext.Contacts
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.ContactId, cancellationToken);
        if (!contactExists)
            throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.ContactId}' not found.");

        var nameTrimmed = request.Name.Trim();
        var duplicateExists = await _dbContext.ContactTags
            .AsNoTracking()
            .Where(t => t.ContactId == request.ContactId && t.Name.ToLower() == nameTrimmed.ToLower())
            .AnyAsync(cancellationToken);
        if (duplicateExists)
            throw new OperationFailedException(ErrorCodes.CONTACT_DUPLICATE_TAG, $"Tag '{request.Name}' already exists on this contact.");

        var tag = ContactTag.Create(request.ContactId, nameTrimmed);
        _dbContext.ContactTags.Add(tag);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return tag.Id;
    }
}
