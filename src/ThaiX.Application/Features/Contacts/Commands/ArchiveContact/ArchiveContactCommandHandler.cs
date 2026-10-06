using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Contacts.Commands.ArchiveContact;

/// <summary>
/// Handler for ArchiveContactCommand.
/// </summary>
public sealed class ArchiveContactCommandHandler : IRequestHandler<ArchiveContactCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public ArchiveContactCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(ArchiveContactCommand request, CancellationToken cancellationToken)
    {
        var contact = await _dbContext.Contacts
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.Id}' not found.");

        if (request.Archive)
            contact.Archive();
        else
            contact.Unarchive();

        await _dbContext.SaveChangesAsync(cancellationToken);

        return contact.Id;
    }
}
