using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Presentation.Models;

public sealed class UpdateJsonBinRequest
{
    /// <summary>Optional. When blank, the existing code is kept.</summary>
    public string? Code { get; init; }

    public required string Name { get; init; }
    public JsonBinCategories Category { get; init; }
    public string? ContentJson { get; init; }
    public string ContentType { get; init; } = "application/json";
    public bool? Compress { get; init; }
    public string? Tags { get; init; }
    public Guid? ReferenceId { get; init; }
    public DateTime? ExpiredAtUtc { get; init; }
    public bool ClearExpiration { get; init; }
}

public sealed class GenerateShareLinkRequest
{
    /// <summary>Optional share-link expiry. Null means no share expiry until revoke.</summary>
    public DateTime? ShareExpiresAtUtc { get; init; }
}

public sealed class JsonBinQueryParameters
{
    public int? PageNumber { get; init; }
    public int? PageSize { get; init; }
    public string? SearchTerm { get; init; }
    public JsonBinCategories? Category { get; init; }
    public Guid? ReferenceId { get; init; }
    public bool? IncludeExpired { get; init; }
    public string? SortBy { get; init; }
    public bool? SortDescending { get; init; }
}
