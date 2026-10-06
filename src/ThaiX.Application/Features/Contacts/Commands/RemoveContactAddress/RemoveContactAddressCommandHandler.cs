using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactAddress;

/// <summary>
/// Removes a contact address by querying/deleting ContactAddresses directly without loading the Contact aggregate.
/// </summary>
public sealed class RemoveContactAddressCommandHandler : IRequestHandler<RemoveContactAddressCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public RemoveContactAddressCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(RemoveContactAddressCommand request, CancellationToken cancellationToken)
    {
        var rowsDeleted = await _dbContext.ContactAddresses
            .Where(a => a.Id == request.AddressId && a.ContactId == request.ContactId)
            .ExecuteDeleteAsync(cancellationToken);

        if (rowsDeleted == 0)
            throw new OperationFailedException(ErrorCodes.CONTACT_ITEM_NOT_FOUND, $"Address with ID '{request.AddressId}' not found for this contact.");

        return request.ContactId;
    }
}
