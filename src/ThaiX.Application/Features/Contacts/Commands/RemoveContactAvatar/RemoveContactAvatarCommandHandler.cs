using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Outbox;
using ThaiX.Domain.Common.Events;

namespace ThaiX.Application.Features.Contacts.Commands.RemoveContactAvatar;

/// <summary>
/// Deletes avatar file from storage and clears Contact.AvatarUrl.
/// </summary>
public sealed class RemoveContactAvatarCommandHandler : IRequestHandler<RemoveContactAvatarCommand, Guid>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IFileStorage _fileStorage;

    public RemoveContactAvatarCommandHandler(
        IApplicationDbContext dbContext,
        IFileStorage fileStorage)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
    }

    public async Task<Guid> Handle(RemoveContactAvatarCommand request, CancellationToken cancellationToken)
    {
        var contact = await _dbContext.Contacts
            .FirstOrDefaultAsync(c => c.Id == request.ContactId, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.ContactId}' not found.");

        var key = $"contacts/{request.ContactId}/avatar/avatar.webp";
        try
        {
            await _fileStorage.DeleteAsync(key, cancellationToken);
        }
        catch
        {
            // File may not exist; still clear the URL
        }

        contact.SetAvatar(null);

        var fullName = contact.FullName.FirstName + " " + contact.FullName.LastName;
        var evt = new ContactProfileUpdatedEvent(contact.Id, fullName.Trim(), null);
        _dbContext.OutboxMessages.Add(new OutboxMessage(
            evt.GetType().FullName!,
            JsonSerializer.Serialize(evt, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase })));

        await _dbContext.SaveChangesAsync(cancellationToken);

        return request.ContactId;
    }
}
