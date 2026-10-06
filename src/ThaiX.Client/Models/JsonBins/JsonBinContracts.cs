namespace ThaiX.Client.Models.JsonBins;

/// <summary>Matches the server-side JsonBinCategories contract (serialized as PascalCase strings).</summary>
public enum JsonBinCategories
{
    Unknown = 0,
    ExternalData = 1,
    Cache = 2,
    Config = 3,
    Temp = 4
}

public class JsonBinListItemModel
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public JsonBinCategories Category { get; set; } = JsonBinCategories.Unknown;
    public string ContentType { get; set; } = "application/json";
    public long SizeBytes { get; set; }
    public bool IsCompressed { get; set; }
    public string? Tags { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTime? ExpiredAtUtc { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class JsonBinDetailModel : JsonBinListItemModel
{
    public string? ContentJson { get; set; }
    public string? ShareToken { get; set; }
    public DateTime? ShareExpiresAtUtc { get; set; }
    public bool IsShared { get; set; }
}

public sealed class GenerateShareLinkRequestModel
{
    public DateTime? ShareExpiresAtUtc { get; set; }
}

public sealed class GenerateShareLinkResultModel
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
    public string RelativePath { get; set; } = string.Empty;
    public DateTime? ShareExpiresAtUtc { get; set; }
}

public sealed class JsonBinSharedModel
{
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string ContentType { get; set; } = "application/json";
    public string ContentJson { get; set; } = string.Empty;
    public DateTime? ShareExpiresAtUtc { get; set; }
}

public sealed class CreateJsonBinModel
{
    /// <summary>Optional. Leave blank to auto-generate a category-prefixed code.</summary>
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public JsonBinCategories Category { get; set; }
    public string ContentJson { get; set; } = "{}";
    public string ContentType { get; set; } = "application/json";
    public bool Compress { get; set; }
    public string? Tags { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTime? ExpiredAtUtc { get; set; }
}

public sealed class UpdateJsonBinModel
{
    /// <summary>Optional. Leave blank to keep the existing code.</summary>
    public string? Code { get; set; }
    public string Name { get; set; } = string.Empty;
    public JsonBinCategories Category { get; set; }
    public string? ContentJson { get; set; }
    public string ContentType { get; set; } = "application/json";
    public bool? Compress { get; set; }
    public string? Tags { get; set; }
    public Guid? ReferenceId { get; set; }
    public DateTime? ExpiredAtUtc { get; set; }
    public bool ClearExpiration { get; set; }
}

public sealed class JsonBinSearchRequest
{
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SearchTerm { get; set; }
    public JsonBinCategories? Category { get; set; }
    public Guid? ReferenceId { get; set; }
    public bool? IncludeExpired { get; set; }
    public string? SortBy { get; set; }
    public bool SortDescending { get; set; }
}

public sealed class ValidateJsonBinCodeModel
{
    public string Code { get; set; } = string.Empty;
    public bool Exists { get; set; }
    public bool IsAvailable { get; set; }
}
