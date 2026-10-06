using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactPhone;

/// <summary>
/// Removes a contact phone by querying/deleting ContactPhones directly without loading the Contact aggregate.
/// </summary>
public sealed class RemoveContactPhoneCommandHandler : IRequestHandler<RemoveContactPhoneCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public RemoveContactPhoneCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(RemoveContactPhoneCommand request, CancellationToken cancellationToken)
    {
        var rowsDeleted = await _dbContext.ContactPhones
            .Where(p => p.Id == request.PhoneId && p.ContactId == request.ContactId)
            .ExecuteDeleteAsync(cancellationToken);

        if (rowsDeleted == 0)
            throw new OperationFailedException(ErrorCodes.CONTACT_ITEM_NOT_FOUND, $"Phone with ID '{request.PhoneId}' not found for this contact.");

        return request.ContactId;
    }
}
