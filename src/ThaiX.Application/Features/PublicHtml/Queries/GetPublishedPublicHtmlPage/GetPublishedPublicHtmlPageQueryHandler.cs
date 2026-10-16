using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.PublicHtml.Common;
using ThaiX.Domain.Aggregates.PublicHtml;

namespace ThaiX.Application.Features.PublicHtml.Queries.GetPublishedPublicHtmlPage;

public sealed class GetPublishedPublicHtmlPageQueryHandler(IApplicationDbContext dbContext)
    : IRequestHandler<GetPublishedPublicHtmlPageQuery, PublicHtmlPageDto?>
{
    public async Task<PublicHtmlPageDto?> Handle(
        GetPublishedPublicHtmlPageQuery request,
        CancellationToken cancellationToken)
    {
        string slug;
        try { slug = PublicHtmlPage.NormalizeSlug(request.Slug); }
        catch (ArgumentException) { return null; }

        return await dbContext.PublicHtmlPages
            .AsNoTracking()
            .Where(x => x.IsPublished && x.Slug == slug)
            .Select(x => new PublicHtmlPageDto(
                x.Id,
                x.Title,
                x.Slug,
                x.SizeBytes,
                x.IsPublished,
                x.CreatedAt,
                x.PublishedAtUtc,
                x.LastViewedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
