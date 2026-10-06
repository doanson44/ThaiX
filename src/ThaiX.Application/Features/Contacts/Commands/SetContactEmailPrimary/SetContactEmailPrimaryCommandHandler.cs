using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Contacts.Commands.SetContactEmailPrimary;

/// <summary>
/// Sets an existing contact email as primary by clearing other primaries then setting this one.
/// </summary>
public sealed class SetContactEmailPrimaryCommandHandler : IRequestHandler<SetContactEmailPrimaryCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public SetContactEmailPrimaryCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(SetContactEmailPrimaryCommand request, CancellationToken cancellationToken)
    {
        var emailBelongsToContact = await _dbContext.ContactEmails
            .AnyAsync(e => e.Id == request.EmailId && e.ContactId == request.ContactId, cancellationToken);
        if (!emailBelongsToContact)
            throw new OperationFailedException(ErrorCodes.CONTACT_ITEM_NOT_FOUND, "Email not found for this contact.");

        await _dbContext.ContactEmails
            .Where(e => e.ContactId == request.ContactId)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.IsPrimary, false), cancellationToken);

        await _dbContext.ContactEmails
            .Where(e => e.Id == request.EmailId && e.ContactId == request.ContactId)
            .ExecuteUpdateAsync(s => s.SetProperty(e => e.IsPrimary, true), cancellationToken);

        return request.ContactId;
    }
}
