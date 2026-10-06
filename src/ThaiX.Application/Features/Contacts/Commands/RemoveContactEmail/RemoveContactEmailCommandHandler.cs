using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactEmail;

/// <summary>
/// Removes a contact email by querying/deleting ContactEmails directly without loading the Contact aggregate.
/// </summary>
public sealed class RemoveContactEmailCommandHandler : IRequestHandler<RemoveContactEmailCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public RemoveContactEmailCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(RemoveContactEmailCommand request, CancellationToken cancellationToken)
    {
        var rowsDeleted = await _dbContext.ContactEmails
            .Where(e => e.Id == request.EmailId && e.ContactId == request.ContactId)
            .ExecuteDeleteAsync(cancellationToken);

        if (rowsDeleted == 0)
            throw new OperationFailedException(ErrorCodes.CONTACT_ITEM_NOT_FOUND, $"Email with ID '{request.EmailId}' not found for this contact.");

        return request.ContactId;
    }
}
