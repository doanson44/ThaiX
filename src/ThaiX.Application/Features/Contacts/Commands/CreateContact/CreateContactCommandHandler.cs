using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Application.Features.Contacts.Commands.CreateContact;

/// <summary>
/// Handler for CreateContactCommand.
/// </summary>
public sealed class CreateContactCommandHandler : IRequestHandler<CreateContactCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;

    public CreateContactCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(CreateContactCommand request, CancellationToken cancellationToken)
    {
        var contact = Contact.Create(
            request.FirstName,
            request.LastName,
            request.Company,
            request.JobTitle,
            request.AvatarUrl,
            request.Birthday,
            request.Notes);

        _dbContext.Contacts.Add(contact);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return contact.Id;
    }
}
