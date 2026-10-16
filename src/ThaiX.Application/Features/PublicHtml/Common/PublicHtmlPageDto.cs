namespace ThaiX.Application.Features.PublicHtml.Common;

public sealed record PublicHtmlPageDto(
    Guid Id,
    string Title,
    string Slug,
    IReadOnlyCollection<string> PreviousSlugs,
    long SizeBytes,
    bool IsPublished,
    DateTime CreatedAtUtc,
    DateTime? PublishedAtUtc,
    DateTime? LastViewedAtUtc);
