using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Application.Features.Contacts.Commands.AddContactAddress;

/// <summary>
/// Queries/updates ContactAddresses directly without loading the Contact aggregate.
/// </summary>
public sealed class AddContactAddressCommandHandler : IRequestHandler<AddContactAddressCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public AddContactAddressCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(AddContactAddressCommand request, CancellationToken cancellationToken)
    {
        var contactExists = await _dbContext.Contacts
            .AsNoTracking()
            .AnyAsync(c => c.Id == request.ContactId, cancellationToken);
        if (!contactExists)
            throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.ContactId}' not found.");

        if (request.IsPrimary)
        {
            await _dbContext.ContactAddresses
                .Where(a => a.ContactId == request.ContactId)
                .ExecuteUpdateAsync(s => s.SetProperty(a => a.IsPrimary, false), cancellationToken);
        }

        var address = ContactAddress.Create(
            request.ContactId,
            request.Street,
            request.CountryCode,
            request.CityCode,
            request.DistrictCode,
            request.PostalCode,
            request.IsPrimary);
        _dbContext.ContactAddresses.Add(address);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return address.Id;
    }
}
