using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.Files.Queries.GetFile;

/// <summary>
/// Retrieves FileAttachment by Id, projects to FileDto with public URL. AsNoTracking.
/// </summary>
public sealed class GetFileQueryHandler : IRequestHandler<GetFileQuery, FileDto?>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IFileUrlProvider _urlProvider;

    public GetFileQueryHandler(IApplicationDbContext dbContext, IFileUrlProvider urlProvider)
    {
        _dbContext = dbContext;
        _urlProvider = urlProvider;
    }

    public async Task<FileDto?> Handle(GetFileQuery request, CancellationToken cancellationToken)
    {
        var meta = await _dbContext.FileAttachments
            .AsNoTracking()
            .Where(f => f.Id == request.FileId)
            .Select(f => new { f.Id, f.FileName, f.ContentType, f.Size, f.StorageKey })
            .FirstOrDefaultAsync(cancellationToken);

        if (meta is null)
            return null;

        return new FileDto
        {
            Id = meta.Id,
            FileName = meta.FileName,
            ContentType = meta.ContentType,
            Size = meta.Size,
            Url = _urlProvider.GetUrl(meta.StorageKey)
        };
    }
}
