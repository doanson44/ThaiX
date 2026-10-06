using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.Files.Queries.GetFile;

/// <summary>
/// Retrieves file metadata and public URL by file ID.
/// </summary>
public sealed record GetFileQuery : IAppQuery<FileDto?>
{
    public Guid FileId { get; init; }
}
