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
                var slug = request.Slug.Trim().ToLowerInvariant();
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
}
