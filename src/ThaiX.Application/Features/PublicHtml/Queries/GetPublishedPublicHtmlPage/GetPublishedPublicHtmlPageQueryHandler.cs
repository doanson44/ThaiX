using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.PublicHtml.Queries.GetPublishedPublicHtmlPage;

public sealed class GetPublishedPublicHtmlPageQueryHandler(IPublicHtmlPageStore pages)
    : IRequestHandler<GetPublishedPublicHtmlPageQuery, PublicHtmlPageDto?>
{
    public Task<PublicHtmlPageDto?> Handle(
        GetPublishedPublicHtmlPageQuery request,
        CancellationToken cancellationToken) =>
        pages.GetPublishedBySlugAsync(request.Slug, cancellationToken);
}
