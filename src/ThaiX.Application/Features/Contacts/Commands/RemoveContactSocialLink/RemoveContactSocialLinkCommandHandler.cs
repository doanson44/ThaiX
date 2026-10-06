using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactSocialLink;

/// <summary>
/// Removes a contact social link by querying/deleting ContactSocialLinks directly without loading the Contact aggregate.
/// </summary>
public sealed class RemoveContactSocialLinkCommandHandler : IRequestHandler<RemoveContactSocialLinkCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public RemoveContactSocialLinkCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(RemoveContactSocialLinkCommand request, CancellationToken cancellationToken)
    {
        var rowsDeleted = await _dbContext.ContactSocialLinks
            .Where(s => s.Id == request.SocialLinkId && s.ContactId == request.ContactId)
            .ExecuteDeleteAsync(cancellationToken);

        if (rowsDeleted == 0)
            throw new OperationFailedException(ErrorCodes.CONTACT_ITEM_NOT_FOUND, $"Social link with ID '{request.SocialLinkId}' not found for this contact.");

        return request.SocialLinkId;
    }
}
