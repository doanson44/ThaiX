using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Identity.Commands.UnlinkContactFromUser;

/// <summary>
/// Removes UserContactLink and UserProfile for the user.
/// </summary>
public sealed class UnlinkContactFromUserCommandHandler : IRequestHandler<UnlinkContactFromUserCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public UnlinkContactFromUserCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(UnlinkContactFromUserCommand request, CancellationToken cancellationToken)
    {
        var link = await _dbContext.UserContactLinks
            .FirstOrDefaultAsync(l => l.UserId == request.UserId, cancellationToken);
        if (link is null)
            throw new OperationFailedException(ErrorCodes.RESOURCE_NOT_FOUND, "No contact is linked to this user.");

        _dbContext.UserContactLinks.Remove(link);

        var profile = await _dbContext.UserProfiles.FirstOrDefaultAsync(p => p.UserId == request.UserId, cancellationToken);
        if (profile is not null)
            _dbContext.UserProfiles.Remove(profile);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
