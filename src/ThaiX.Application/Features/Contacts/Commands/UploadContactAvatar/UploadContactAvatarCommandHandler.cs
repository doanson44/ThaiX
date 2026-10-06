using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.Outbox;
using ThaiX.Domain.Common.Events;

namespace ThaiX.Application.Features.Contacts.Commands.UploadContactAvatar;

/// <summary>
/// Validates image type and size, saves via IFileStorage, updates Contact.AvatarUrl via SetAvatar.
/// </summary>
public sealed class UploadContactAvatarCommandHandler : IRequestHandler<UploadContactAvatarCommand, string>
{
    private const long MaxAvatarSizeBytes = 2 * 1024 * 1024; // 2MB
    private static readonly string[] AllowedContentTypes = { "image/jpeg", "image/png", "image/webp" };
    private static readonly JsonSerializerOptions JsonOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly IApplicationDbContext _dbContext;
    private readonly IFileStorage _fileStorage;
    private readonly IFileUrlProvider _urlProvider;

    public UploadContactAvatarCommandHandler(
        IApplicationDbContext dbContext,
        IFileStorage fileStorage,
        IFileUrlProvider urlProvider)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
        _urlProvider = urlProvider;
    }

    public async Task<string> Handle(UploadContactAvatarCommand request, CancellationToken cancellationToken)
    {
        var contact = await _dbContext.Contacts
            .FirstOrDefaultAsync(c => c.Id == request.ContactId, cancellationToken)
            ?? throw new OperationFailedException(ErrorCodes.CONTACT_NOT_FOUND, $"Contact with ID '{request.ContactId}' not found.");

        var contentType = request.ContentType?.Trim().ToLowerInvariant() ?? string.Empty;
        if (!AllowedContentTypes.Contains(contentType, StringComparer.OrdinalIgnoreCase))
            throw new OperationFailedException(ErrorCodes.FILE_TYPE_NOT_ALLOWED, "Avatar must be image/jpeg, image/png, or image/webp.");

        if (request.FileStream.CanSeek && request.FileStream.Length > MaxAvatarSizeBytes)
            throw new OperationFailedException(ErrorCodes.FILE_TOO_LARGE, "Avatar must not exceed 2 MB.");

        var key = $"contacts/{request.ContactId}/avatar/avatar.webp";
        await _fileStorage.SaveAsync(request.FileStream, key, contentType, cancellationToken);
        var url = _urlProvider.GetUrl(key);
        contact.SetAvatar(url);

        var fullName = contact.FullName.FirstName + " " + contact.FullName.LastName;
        var evt = new ContactProfileUpdatedEvent(contact.Id, fullName.Trim(), url);
        _dbContext.OutboxMessages.Add(new OutboxMessage(
            evt.GetType().FullName!,
            JsonSerializer.Serialize(evt, JsonOptions)));

        await _dbContext.SaveChangesAsync(cancellationToken);
        return url;
    }
}
