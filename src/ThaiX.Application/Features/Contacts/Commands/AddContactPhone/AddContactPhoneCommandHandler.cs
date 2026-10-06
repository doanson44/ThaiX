using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactPhone;

/// <summary>
/// Adds a phone by mutating ContactPhones directly to avoid aggregate concurrency conflicts.
/// </summary>
public sealed class AddContactPhoneCommandHandler : IRequestHandler<AddContactPhoneCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public AddContactPhoneCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(AddContactPhoneCommand request, CancellationToken cancellationToken)
    {
        var contactExists = await _dbContext.Contacts
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.ContactId, cancellationToken);
        if (!contactExists)
            throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.ContactId}' not found.");

        if (request.IsPrimary)
        {
            await _dbContext.ContactPhones
                .Where(p => p.ContactId == request.ContactId)
                .ExecuteUpdateAsync(s => s.SetProperty(p => p.IsPrimary, false), cancellationToken);
        }

        var phone = ContactPhone.Create(request.ContactId, request.Value, request.IsPrimary);
        _dbContext.ContactPhones.Add(phone);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return phone.Id;
    }
}
