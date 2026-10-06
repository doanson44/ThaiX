using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;
using ThaiX.Domain.Common.Entities;

namespace ThaiX.Application.Features.Files.Commands.UploadFile;

/// <summary>
/// Saves file via IFileStorage, creates FileAttachment record, returns DTO with URL.
/// </summary>
public sealed class UploadFileCommandHandler : IRequestHandler<UploadFileCommand, FileDto>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IFileStorage _fileStorage;
    private readonly IFileUrlProvider _urlProvider;

    public UploadFileCommandHandler(
        IApplicationDbContext dbContext,
        IFileStorage fileStorage,
        IFileUrlProvider urlProvider)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
        _urlProvider = urlProvider;
    }

    public async Task<FileDto> Handle(UploadFileCommand request, CancellationToken cancellationToken)
    {
        var key = await _fileStorage.SaveAsync(
            request.FileStream,
            request.StorageKey,
            request.ContentType,
            cancellationToken);

        var size = request.FileStream.CanSeek ? request.FileStream.Length : 0L;
        var attachment = FileAttachment.Create(key, request.FileName, request.ContentType, size);
        _dbContext.FileAttachments.Add(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return new FileDto
        {
            Id = attachment.Id,
            FileName = attachment.FileName,
            ContentType = attachment.ContentType,
            Size = attachment.Size,
            Url = _urlProvider.GetUrl(attachment.StorageKey)
        };
    }
}
