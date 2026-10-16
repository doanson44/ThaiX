using MediatR;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.PublicHtml.Common;

namespace ThaiX.Application.Features.PublicHtml.Queries.OpenPublishedPublicHtmlContent;

public sealed class OpenPublishedPublicHtmlContentQueryHandler(IFileStorage storage)
    : IRequestHandler<OpenPublishedPublicHtmlContentQuery, Stream?>
{
    public async Task<Stream?> Handle(
        OpenPublishedPublicHtmlContentQuery request,
        CancellationToken cancellationToken)
    {
        string slug;
        try { slug = NormalizeSlug(request.Slug); }
        catch (ArgumentException) { return null; }

        var storageKey = await PublicHtmlPageIndex.ExecuteAsync(
            storage,
            (pages, _) =>
            {
                var page = pages.FirstOrDefault(x => x.IsPublished &&
                    (x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) ||
                     x.PreviousSlugs.Contains(slug, StringComparer.OrdinalIgnoreCase)));
                return Task.FromResult((page?.StorageKey, false));
            },
            cancellationToken);

        if (storageKey is null)
            return null;

        try { return await storage.GetAsync(storageKey, cancellationToken); }
        catch (OperationFailedException) { return null; }
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
}
