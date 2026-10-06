using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Contacts.Commands.SetContactPhonePrimary;

/// <summary>
/// Sets an existing contact phone as primary by clearing other primaries then setting this one.
/// </summary>
public sealed class SetContactPhonePrimaryCommandHandler : IRequestHandler<SetContactPhonePrimaryCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public SetContactPhonePrimaryCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(SetContactPhonePrimaryCommand request, CancellationToken cancellationToken)
    {
        var phoneBelongsToContact = await _dbContext.ContactPhones
            .AnyAsync(p => p.Id == request.PhoneId && p.ContactId == request.ContactId, cancellationToken);
        if (!phoneBelongsToContact)
            throw new OperationFailedException(ErrorCodes.CONTACT_ITEM_NOT_FOUND, "Phone not found for this contact.");

        await _dbContext.ContactPhones
            .Where(p => p.ContactId == request.ContactId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsPrimary, false), cancellationToken);

        await _dbContext.ContactPhones
            .Where(p => p.Id == request.PhoneId && p.ContactId == request.ContactId)
            .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsPrimary, true), cancellationToken);

        return request.ContactId;
    }
}
