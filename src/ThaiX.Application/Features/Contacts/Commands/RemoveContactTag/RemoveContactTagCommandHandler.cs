using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactTag;

/// <summary>
/// Removes a contact tag by querying/deleting ContactTags directly without loading the Contact aggregate.
/// </summary>
public sealed class RemoveContactTagCommandHandler : IRequestHandler<RemoveContactTagCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public RemoveContactTagCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(RemoveContactTagCommand request, CancellationToken cancellationToken)
    {
        var rowsDeleted = await _dbContext.ContactTags
            .Where(t => t.Id == request.TagId && t.ContactId == request.ContactId)
            .ExecuteDeleteAsync(cancellationToken);

        if (rowsDeleted == 0)
            throw new OperationFailedException(ErrorCodes.CONTACT_ITEM_NOT_FOUND, $"Tag with ID '{request.TagId}' not found for this contact.");

        return request.TagId;
    }
}
