using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Contacts.Commands.DeleteContact;

/// <summary>
/// Handler for DeleteContactCommand.
/// </summary>
public sealed class DeleteContactCommandHandler : IRequestHandler<DeleteContactCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteContactCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(DeleteContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _dbContext.Contacts
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.Id}' not found.");

        contact.SoftDelete();
        await _dbContext.SaveChangesAsync(cancellationToken);

        return contact.Id;
    }
}
