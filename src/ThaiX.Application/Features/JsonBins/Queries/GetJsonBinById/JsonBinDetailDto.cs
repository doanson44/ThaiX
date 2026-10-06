using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Application.Features.JsonBins.Queries.GetJsonBinById;

/// <summary>
/// Detail DTO -- may include Content as UTF-8 JSON text (decompressed when needed).
/// </summary>
public sealed record JsonBinDetailDto
{
    public required Guid Id { get; init; }
    public required string Code { get; init; }
    public required string Name { get; init; }
    public required JsonBinCategories Category { get; init; }
    public required string ContentType { get; init; }
    public required long SizeBytes { get; init; }
    public required bool IsCompressed { get; init; }
    public string? Tags { get; init; }
    public Guid? ReferenceId { get; init; }
    public DateTime? ExpiredAtUtc { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
    public string? ContentJson { get; init; }
    public string? ShareToken { get; init; }
    public DateTime? ShareExpiresAtUtc { get; init; }
    public bool IsShared { get; init; }
}
