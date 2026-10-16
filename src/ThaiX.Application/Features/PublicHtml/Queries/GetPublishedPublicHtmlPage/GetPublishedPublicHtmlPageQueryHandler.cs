using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.PublicHtml.Common;

namespace ThaiX.Application.Features.PublicHtml.Queries.GetPublishedPublicHtmlPage;

public sealed class GetPublishedPublicHtmlPageQueryHandler(IFileStorage storage)
    : IRequestHandler<GetPublishedPublicHtmlPageQuery, PublicHtmlPageDto?>
{
    public Task<PublicHtmlPageDto?> Handle(
        GetPublishedPublicHtmlPageQuery request,
        CancellationToken cancellationToken)
    {
        string slug;
        try { slug = NormalizeSlug(request.Slug); }
        catch (ArgumentException) { return Task.FromResult<PublicHtmlPageDto?>(null); }

        return PublicHtmlPageIndex.ExecuteAsync(
            storage,
            (pages, _) =>
            {
                var page = pages.FirstOrDefault(x => x.IsPublished &&
                    (x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) ||
                     x.PreviousSlugs.Contains(slug, StringComparer.OrdinalIgnoreCase)));
                return Task.FromResult((page is null ? null : ToDto(page), false));
            },
            cancellationToken);
    }

    private static string NormalizeSlug(string slug)
    {
        slug = (slug ?? string.Empty).Trim().ToLowerInvariant();
        if (slug.Length is < 1 or > 80 ||
            slug.Any(c => !(char.IsAsciiLetterOrDigit(c) || c == '-')) ||
            slug.StartsWith('-') || slug.EndsWith('-') ||
            slug.Contains("--", StringComparison.Ordinal))
            throw new ArgumentException("Invalid slug.", nameof(slug));
        return slug;
    }

    private static PublicHtmlPageDto ToDto(StoredPublicHtmlPage page) =>
        new(page.Id, page.Title, page.Slug, page.PreviousSlugs, page.SizeBytes, page.IsPublished, page.CreatedAtUtc, page.PublishedAtUtc);
}
