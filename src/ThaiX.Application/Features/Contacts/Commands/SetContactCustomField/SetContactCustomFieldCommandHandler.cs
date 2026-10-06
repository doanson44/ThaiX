using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Contacts.Commands.SetContactCustomField;

public sealed class SetContactCustomFieldCommandHandler : IRequestHandler<SetContactCustomFieldCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public SetContactCustomFieldCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(SetContactCustomFieldCommand request, CancellationToken cancellationToken)
    {
        var contact = await _dbContext.Contacts
            .FirstOrDefaultAsync(c => c.Id == request.ContactId, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.ContactId}' not found.");

        contact.SetCustomField(request.Key, request.Value);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return request.ContactId;
    }
}
