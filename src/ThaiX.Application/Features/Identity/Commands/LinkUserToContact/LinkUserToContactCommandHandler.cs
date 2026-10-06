using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Identity;

namespace ThaiX.Application.Features.Identity.Commands.LinkUserToContact;

/// <summary>
/// Creates UserContactLink and initial UserProfile from Contact data.
/// </summary>
public sealed class LinkUserToContactCommandHandler : IRequestHandler<LinkUserToContactCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;

    public LinkUserToContactCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit> Handle(LinkUserToContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _dbContext.Contacts
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == request.ContactId, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.ContactId}' not found.");

        var alreadyLinked = await _dbContext.UserContactLinks
            .AnyAsync(l => l.UserId == request.UserId || l.ContactId == request.ContactId, cancellationToken);
        if (alreadyLinked)
            throw new OperationFailedException(ErrorCodes.DUPLICATE_ENTRY, "User or contact is already linked.");

        var link = UserContactLink.Create(request.UserId, request.ContactId);
        _dbContext.UserContactLinks.Add(link);

        var fullName = contact.FullName.FirstName + " " + contact.FullName.LastName;
        var profile = new UserProfile
        {
            UserId = request.UserId,
            ContactId = request.ContactId,
            FullName = fullName.Trim(),
            AvatarUrl = contact.AvatarUrl,
            UpdatedAt = DateTime.UtcNow
        };
        _dbContext.UserProfiles.Add(profile);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
