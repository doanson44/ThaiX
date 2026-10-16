using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.PublicHtml.Queries.OpenPublishedPublicHtmlContent;

public sealed class OpenPublishedPublicHtmlContentQueryHandler(IPublicHtmlPageService pages)
    : IRequestHandler<OpenPublishedPublicHtmlContentQuery, Stream?>
{
    public Task<Stream?> Handle(
        OpenPublishedPublicHtmlContentQuery request,
        CancellationToken cancellationToken) =>
        pages.OpenPublishedContentAsync(request.Slug, cancellationToken);
}
