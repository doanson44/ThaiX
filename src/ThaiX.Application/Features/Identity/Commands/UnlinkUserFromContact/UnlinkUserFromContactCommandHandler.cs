using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Identity.Commands.UnlinkUserFromContact;

/// <summary>
/// Removes UserContactLink and UserProfile for the contact.
/// </summary>
public sealed class UnlinkUserFromContactCommandHandler : IRequestHandler<UnlinkUserFromContactCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UnlinkUserFromContactCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UnlinkUserFromContactCommand request, CancellationToken cancellationToken)
    {
        var link = await _dbContext.UserContactLinks
            .FirstOrDefaultAsync(l => l.ContactId == request.ContactId, cancellationToken);
        if (link is null)
            throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, "No user is linked to this contact.");

        var userId = link.UserId;
        _dbContext.UserContactLinks.Remove(link);

        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == userId, cancellationToken);
        if (profile is not null)
            _dbContext.UserProfiles.Remove(profile);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
