using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactCustomField;

public sealed class RemoveContactCustomFieldCommandHandler : IRequestHandler<RemoveContactCustomFieldCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public RemoveContactCustomFieldCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(RemoveContactCustomFieldCommand request, CancellationToken cancellationToken)
    {
        var contact = await _dbContext.Contacts
            .FirstOrDefaultAsync(c => c.Id == request.ContactId, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.ContactId}' not found.");

        contact.RemoveCustomField(request.Key);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return contact.Id;
    }
}
