using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.Files.Commands.DeleteFile;

/// <summary>
/// Loads FileAttachment, deletes file from storage, removes record.
/// </summary>
public sealed class DeleteFileCommandHandler : IRequestHandler<DeleteFileCommand, Unit>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IFileStorage _fileStorage;

    public DeleteFileCommandHandler(IApplicationDbContext dbContext, IFileStorage fileStorage)
    {
        _dbContext = dbContext;
        _fileStorage = fileStorage;
    }

    public async Task<Unit> Handle(DeleteFileCommand request, CancellationToken cancellationToken)
    {
        var attachment = await _dbContext.FileAttachments
            .FirstOrDefaultAsync(f => f.Id == request.FileId, cancellationToken);

        if (attachment is null)
            throw new OperationFailedException(ErrorCodes.FILE_NOT_FOUND, $"File with ID '{request.FileId}' not found.");

        await _fileStorage.DeleteAsync(attachment.StorageKey, cancellationToken);
        _dbContext.FileAttachments.Remove(attachment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Unit.Value;
    }
}
