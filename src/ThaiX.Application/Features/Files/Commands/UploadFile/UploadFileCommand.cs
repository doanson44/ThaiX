using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Common.Models;

namespace ThaiX.Application.Features.Files.Commands.UploadFile;

/// <summary>
/// Uploads a file to storage and creates a FileAttachment record.
/// </summary>
public sealed record UploadFileCommand : IAppCommand<FileDto>
{
    public required Stream FileStream { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public required string StorageKey { get; init; }
}
