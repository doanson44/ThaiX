using ThaiX.Client.Models.Blog;

namespace ThaiX.Client.Services.Files;

/// <summary>
/// Client service for file metadata upload helpers (GET/DELETE by id).
/// </summary>
public interface IFileService
{
    Task<UploadedFileDto> GetFileAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteFileAsync(Guid id, CancellationToken cancellationToken = default);

    Task<UploadedFileDto> UploadFileAsync(
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);
}
