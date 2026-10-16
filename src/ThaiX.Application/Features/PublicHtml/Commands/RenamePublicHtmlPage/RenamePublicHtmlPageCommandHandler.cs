using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.PublicHtml.Common;

namespace ThaiX.Application.Features.PublicHtml.Commands.RenamePublicHtmlPage;

public sealed class RenamePublicHtmlPageCommandHandler(IFileStorage storage)
    : IRequestHandler<RenamePublicHtmlPageCommand, Guid>
{
    public Task<Guid> Handle(RenamePublicHtmlPageCommand request, CancellationToken cancellationToken) =>
        PublicHtmlPageIndex.ExecuteAsync(
            storage,
            (pages, _) =>
            {
                var slug = NormalizeSlug(request.Slug);
                var index = pages.FindIndex(x => x.Id == request.Id);
                if (index < 0)
                    throw new KeyNotFoundException("HTML page was not found.");

                if (pages.Any(x => x.Id != request.Id &&
                    (x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) ||
                     x.PreviousSlugs.Contains(slug, StringComparer.OrdinalIgnoreCase))))
                    throw new InvalidOperationException("The slug is already in use.");

                var page = pages[index];
                if (!page.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase))
                    page = page with
                    {
                        PreviousSlugs = page.PreviousSlugs.Append(page.Slug).Distinct(StringComparer.OrdinalIgnoreCase).ToArray(),
                        Slug = slug
                    };

                pages[index] = page;
                return Task.FromResult((request.Id, true));
            },
            cancellationToken);

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
}
