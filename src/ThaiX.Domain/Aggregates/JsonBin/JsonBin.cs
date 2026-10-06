using System.Security.Cryptography;
using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.JsonBin;

/// <summary>
/// Standardized category values for JsonBin entries.
/// Stored as string in the database.
/// </summary>
public enum JsonBinCategories
{
    Unknown,
    ExternalData,
    Cache,
    Config,
    Temp
}

/// <summary>
/// Reusable JSON binary storage aggregate root.
/// State changes only through aggregate methods.
/// </summary>
public sealed class JsonBin : BaseAuditableEntity
{
    public const int CodeMaxLength = 100;
    public const int ShareTokenMaxLength = 64;

    public string Code { get; private set; }
    public string Name { get; private set; }
    public JsonBinCategories Category { get; private set; }
    public byte[] Content { get; private set; }
    public string ContentType { get; private set; }
    public long SizeBytes { get; private set; }
    public bool IsCompressed { get; private set; }
    public string? Tags { get; private set; }
    public Guid? ReferenceId { get; private set; }
    public DateTime? ExpiredAtUtc { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    /// <summary>Unguessable token for anonymous share links. Null when sharing is disabled.</summary>
    public string? ShareToken { get; private set; }

    /// <summary>Optional share-link expiry (independent from bin <see cref="ExpiredAtUtc"/>).</summary>
    public DateTime? ShareExpiresAtUtc { get; private set; }

    private JsonBin()
    {
        Code = string.Empty;
        Name = string.Empty;
        Category = JsonBinCategories.Unknown;
        Content = Array.Empty<byte>();
        ContentType = string.Empty;
        CreatedAtUtc = DateTime.MinValue;
    }

    /// <summary>
    /// Normalizes user-facing codes for uniqueness comparisons.
    /// </summary>
    public static string NormalizeCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code is required.", nameof(code));

        var normalized = code.Trim().ToUpperInvariant();
        if (normalized.Length > CodeMaxLength)
            throw new ArgumentException($"Code must be at most {CodeMaxLength} characters.", nameof(code));

        return normalized;
    }

    /// <summary>
    /// Returns null when the user left code blank (caller should auto-generate).
    /// </summary>
    public static string? NormalizeCodeOrNull(string? code) =>
        string.IsNullOrWhiteSpace(code) ? null : NormalizeCode(code);

    /// <summary>
    /// Auto-generated code including category token, UTC timestamp, and random suffix.
    /// Example: <c>CONFIG_20260730100415789_A1B2C3D4</c>
    /// </summary>
    public static string GenerateCode(JsonBinCategories category)
    {
        var categoryToken = category.ToString().ToUpperInvariant();
        var stamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var suffix = Guid.NewGuid().ToString("N")[..8].ToUpperInvariant();
        return NormalizeCode($"{categoryToken}_{stamp}_{suffix}");
    }

    public static JsonBin Create(
        string code,
        string name,
        JsonBinCategories category,
        byte[] content,
        string contentType = "application/json",
        bool isCompressed = false,
        string? tags = null,
        Guid? referenceId = null,
        DateTime? expiredAtUtc = null,
        Guid? id = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        ArgumentNullException.ThrowIfNull(content);

        var normalizedContentType = string.IsNullOrWhiteSpace(contentType)
            ? "application/json"
            : contentType;

        var nowUtc = DateTime.UtcNow;

        return new JsonBin
        {
            Id = id.GetValueOrDefault(Guid.NewGuid()),
            Code = NormalizeCode(code),
            Name = name.Trim(),
            Category = category,
            Content = content,
            ContentType = normalizedContentType,
            SizeBytes = content.LongLength,
            IsCompressed = isCompressed,
            Tags = tags,
            ReferenceId = referenceId,
            ExpiredAtUtc = expiredAtUtc,
            CreatedAtUtc = nowUtc,
            CreatedAt = nowUtc
        };
    }

    public void ChangeCode(string code) => Code = NormalizeCode(code);

    public void Rename(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        Name = name.Trim();
    }

    public void Update(
        JsonBinCategories category,
        string? tags,
        Guid? referenceId)
    {
        Category = category;
        Tags = tags;
        ReferenceId = referenceId;
    }

    public void ReplaceContent(byte[] content, string contentType, bool isCompressed)
    {
        ArgumentNullException.ThrowIfNull(content);

        Content = content;
        ContentType = string.IsNullOrWhiteSpace(contentType)
            ? "application/json"
            : contentType;
        SizeBytes = content.LongLength;
        IsCompressed = isCompressed;
    }

    public void ChangeExpiration(DateTime? expiredAtUtc) => ExpiredAtUtc = expiredAtUtc;

    public void MarkExpired()
    {
        if (ExpiredAtUtc.HasValue && ExpiredAtUtc.Value <= DateTime.UtcNow)
            return;

        ExpiredAtUtc = DateTime.UtcNow;
        RevokeShare();
    }

    /// <summary>
    /// Enables or regenerates an anonymous share token.
    /// </summary>
    /// <returns>The new share token.</returns>
    public string EnableShare(DateTime? shareExpiresAtUtc = null)
    {
        if (shareExpiresAtUtc.HasValue && shareExpiresAtUtc.Value <= DateTime.UtcNow)
            throw new ArgumentException("Share expiration must be in the future.", nameof(shareExpiresAtUtc));

        ShareToken = CreateShareToken();
        ShareExpiresAtUtc = shareExpiresAtUtc;
        return ShareToken;
    }

    public void RevokeShare()
    {
        ShareToken = null;
        ShareExpiresAtUtc = null;
    }

    public bool IsShareActive(DateTime utcNow) =>
        !IsDeleted
        && !string.IsNullOrEmpty(ShareToken)
        && (!ShareExpiresAtUtc.HasValue || ShareExpiresAtUtc.Value > utcNow)
        && (!ExpiredAtUtc.HasValue || ExpiredAtUtc.Value > utcNow);

    public void SoftDelete()
    {
        RevokeShare();
        Delete();
    }

    private static string CreateShareToken() =>
        Convert.ToHexString(RandomNumberGenerator.GetBytes(16));
}
