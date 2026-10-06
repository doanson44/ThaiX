namespace ThaiX.Application.Common.Interfaces;

/// <summary>
/// Abstraction for file storage. Implement with LocalFileStorage or S3 without changing Application layer.
/// </summary>
public interface IFileStorage
{
    /// <summary>
    /// Saves a file stream under the given key. Returns the key used for storage.
    /// </summary>
    Task<string> SaveAsync(Stream stream, string key, string contentType, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a read stream for the file at the given key.
    /// </summary>
    Task<Stream> GetAsync(string key, CancellationToken cancellationToken = default);

    /// <summary>
    /// Deletes the file at the given key.
    /// </summary>
    Task DeleteAsync(string key, CancellationToken cancellationToken = default);
}
