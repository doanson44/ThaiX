namespace ThaiX.Application.Features.PublicHtml.Common;

public sealed record StoredPublicHtmlPage(
    Guid Id,
    string Title,
    string Slug,
    string[] PreviousSlugs,
    string StorageKey,
    long SizeBytes,
    bool IsPublished,
    DateTime CreatedAtUtc,
    DateTime? PublishedAtUtc);
