namespace ThaiX.Application.Common.Models;

/// <summary>
/// DTO for file metadata and access URL.
/// </summary>
public sealed record FileDto
{
    public Guid Id { get; init; }
    public required string FileName { get; init; }
    public required string ContentType { get; init; }
    public long Size { get; init; }
    public required string Url { get; init; }
}
