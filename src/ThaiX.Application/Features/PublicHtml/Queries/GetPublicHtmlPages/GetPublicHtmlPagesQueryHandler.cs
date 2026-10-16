using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.PublicHtml.Queries.GetPublicHtmlPages;

public sealed class GetPublicHtmlPagesQueryHandler(IPublicHtmlPageStore pages)
    : IRequestHandler<GetPublicHtmlPagesQuery, IReadOnlyCollection<PublicHtmlPageDto>>
{
    public Task<IReadOnlyCollection<PublicHtmlPageDto>> Handle(
        GetPublicHtmlPagesQuery request,
        CancellationToken cancellationToken) =>
        pages.GetAllAsync(cancellationToken);
}
