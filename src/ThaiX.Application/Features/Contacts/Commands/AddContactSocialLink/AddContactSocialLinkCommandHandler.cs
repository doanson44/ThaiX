using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactSocialLink;

/// <summary>
/// Queries/updates ContactSocialLinks directly without loading the Contact aggregate.
/// </summary>
public sealed class AddContactSocialLinkCommandHandler : IRequestHandler<AddContactSocialLinkCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public AddContactSocialLinkCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(AddContactSocialLinkCommand request, CancellationToken cancellationToken)
    {
        var contactExists = await _dbContext.Contacts
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.ContactId, cancellationToken);
        if (!contactExists)
            throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.ContactId}' not found.");

        var link = ContactSocialLink.Create(request.ContactId, request.Platform, request.Url);
        _dbContext.ContactSocialLinks.Add(link);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return link.Id;
    }
}
