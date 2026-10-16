using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.PublicHtml.Common;

namespace ThaiX.Application.Features.PublicHtml.Queries.GetPublicHtmlPages;

public sealed class GetPublicHtmlPagesQueryHandler(IFileStorage storage)
    : IRequestHandler<GetPublicHtmlPagesQuery, IReadOnlyCollection<PublicHtmlPageDto>>
{
    public Task<IReadOnlyCollection<PublicHtmlPageDto>> Handle(
        GetPublicHtmlPagesQuery request,
        CancellationToken cancellationToken) =>
        PublicHtmlPageIndex.ExecuteAsync(
            storage,
            (pages, _) => Task.FromResult((
                (IReadOnlyCollection<PublicHtmlPageDto>)pages
                    .OrderByDescending(x => x.CreatedAtUtc)
                    .Select(ToDto)
                    .ToArray(),
                false)),
            cancellationToken);

    private static PublicHtmlPageDto ToDto(StoredPublicHtmlPage page) =>
        new(page.Id, page.Title, page.Slug, page.PreviousSlugs, page.SizeBytes, page.IsPublished, page.CreatedAtUtc, page.PublishedAtUtc);
}
