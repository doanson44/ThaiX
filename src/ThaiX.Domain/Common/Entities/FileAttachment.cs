namespace ThaiX.Domain.Common.Entities;

/// <summary>
/// Metadata for a stored file. Actual file content is in storage (local or S3).
/// </summary>
public sealed class FileAttachment : BaseAuditableEntity
{
    public string StorageKey { get; private set; } = string.Empty;
    public string FileName { get; private set; } = string.Empty;
    public string ContentType { get; private set; } = string.Empty;
    public long Size { get; private set; }

    private FileAttachment() { }

    public static FileAttachment Create(string storageKey, string fileName, string contentType, long size)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey, nameof(storageKey));
        ArgumentException.ThrowIfNullOrWhiteSpace(fileName, nameof(fileName));
        ArgumentException.ThrowIfNullOrWhiteSpace(contentType, nameof(contentType));
        if (size < 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Size must be non-negative.");

        return new FileAttachment
        {
            Id = Guid.NewGuid(),
            StorageKey = storageKey.Trim(),
            FileName = fileName.Trim(),
            ContentType = contentType.Trim().ToLowerInvariant(),
            Size = size
        };
    }
}
