using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.PublicHtml.Common;

namespace ThaiX.Application.Features.PublicHtml.Queries.GetPublicHtmlPages;

public sealed class GetPublicHtmlPagesQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetPublicHtmlPagesQuery, IReadOnlyCollection<PublicHtmlPageDto>>
{
    public async Task<IReadOnlyCollection<PublicHtmlPageDto>> Handle(
        GetPublicHtmlPagesQuery request,
        CancellationToken cancellationToken) =>
        await dbContext.PublicHtmlPages
            .AsNoTracking()
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new PublicHtmlPageDto(
                x.Id,
                x.Title,
                x.Slug,
                x.SizeBytes,
                x.IsPublished,
                x.CreatedAt,
                x.PublishedAtUtc,
                x.LastViewedAtUtc))
            .ToListAsync(cancellationToken);
}
