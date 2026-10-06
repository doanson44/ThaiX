using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Outbox;
using ThaiX.Domain.Common.Events;

namespace ThaiX.Application.Features.Contacts.Commands.UpdateContactProfile;

/// <summary>
/// Handler for UpdateContactProfileCommand.
/// </summary>
public sealed class UpdateContactProfileCommandHandler : IRequestHandler<UpdateContactProfileCommand, Guid>
{
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly IApplicationDbContext _dbContext;

    public UpdateContactProfileCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Guid> Handle(UpdateContactProfileCommand request, CancellationToken cancellationToken)
    {
        var contact = await _dbContext.Contacts
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.Id}' not found.");

        contact.UpdateProfile(
            request.FirstName,
            request.LastName,
            request.Company,
            request.JobTitle,
            request.AvatarUrl,
            request.Birthday,
            request.Notes);

        var fullName = contact.FullName.FirstName + " " + contact.FullName.LastName;
        var evt = new ContactProfileUpdatedEvent(contact.Id, fullName.Trim(), contact.AvatarUrl);
        _dbContext.OutboxMessages.Add(new OutboxMessage(
            evt.GetType().FullName!,
            JsonSerializer.Serialize(evt, JsonOptions)));

        await _dbContext.SaveChangesAsync(cancellationToken);

        return contact.Id;
    }
}
