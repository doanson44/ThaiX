using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.PublicHtml.Common;

namespace ThaiX.Application.Features.PublicHtml.Commands.UploadPublicHtmlPage;

public sealed class UploadPublicHtmlPageCommandHandler(IFileStorage storage)
    : IRequestHandler<UploadPublicHtmlPageCommand, Guid>
{
    private const string ContentPrefix = "public-html/content";
    private const long MaxSizeBytes = 5 * 1024 * 1024;

    public async Task<Guid> Handle(UploadPublicHtmlPageCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var storageKey = $"{ContentPrefix}/{id:N}.html";
        var contentSaved = false;

        try
        {
            return await PublicHtmlPageIndex.ExecuteAsync(
                storage,
                async (pages, ct) =>
                {
                    var slug = NormalizeSlug(request.Slug);
                    EnsureSlugAvailable(pages, slug, null);
                    if (request.SizeBytes is <= 0 or > MaxSizeBytes)
                        throw new ArgumentOutOfRangeException(nameof(request.SizeBytes), "HTML files must be between 1 byte and 5 MB.");

                    var title = string.IsNullOrWhiteSpace(request.Title)
                        ? Path.GetFileNameWithoutExtension(request.FileName)
                        : request.Title.Trim();
                    if (title.Length > 200)
                        throw new ArgumentException("Title must not exceed 200 characters.", nameof(request.Title));

                    await storage.SaveAsync(request.Content, storageKey, "text/html", ct);
                    contentSaved = true;
                    pages.Add(new StoredPublicHtmlPage(
                        id, title, slug, [], storageKey, request.SizeBytes, false, DateTime.UtcNow, null));
                    return (id, true);
                },
                cancellationToken);
        }
        catch
        {
            if (contentSaved)
                await storage.DeleteAsync(storageKey, cancellationToken);
            throw;
        }
    }

    private static string NormalizeSlug(string slug)
    {
        slug = (slug ?? string.Empty).Trim().ToLowerInvariant();
        if (slug.Length is < 1 or > 80 ||
            slug.Any(c => !(char.IsAsciiLetterOrDigit(c) || c == '-')) ||
            slug.StartsWith('-') || slug.EndsWith('-') ||
            slug.Contains("--", StringComparison.Ordinal))
            throw new ArgumentException("Slug must contain 1-80 lowercase letters, numbers, or single hyphens.", nameof(slug));
        return slug;
    }

    private static void EnsureSlugAvailable(IEnumerable<StoredPublicHtmlPage> pages, string slug, Guid? currentId)
    {
        if (pages.Any(x => x.Id != currentId &&
            (x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) ||
             x.PreviousSlugs.Contains(slug, StringComparer.OrdinalIgnoreCase))))
            throw new InvalidOperationException("The slug is already in use.");
    }
}
