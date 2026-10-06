using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactEmail;

/// <summary>
/// Handler for AddContactEmailCommand.
/// Queries/updates ContactEmails directly without loading the Contact aggregate.
/// </summary>
public sealed class AddContactEmailCommandHandler : IRequestHandler<AddContactEmailCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public AddContactEmailCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(AddContactEmailCommand request, CancellationToken cancellationToken)
    {
        var contactExists = await _dbContext.Contacts
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.ContactId, cancellationToken);
        if (!contactExists)
            throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.ContactId}' not found.");

        if (request.IsPrimary)
        {
            await _dbContext.ContactEmails
                .Where(e => e.ContactId == request.ContactId)
                .ExecuteUpdateAsync(s => s.SetProperty(e => e.IsPrimary, false), cancellationToken);
        }

        var email = ContactEmail.Create(request.ContactId, request.Value, request.IsPrimary);
        _dbContext.ContactEmails.Add(email);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return email.Id;
    }
}
