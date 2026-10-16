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
            .Where(x => x.IsPublished && x.Slugs.Any(s => s.Slug == slug))
            .Select(x => new PublicHtmlPageDto(
                x.Id,
                x.Title,
                x.Slugs.Where(s => s.IsCanonical).Select(s => s.Slug).FirstOrDefault() ?? string.Empty,
                x.Slugs.Where(s => !s.IsCanonical).Select(s => s.Slug).ToArray(),
                x.SizeBytes,
                x.IsPublished,
                x.CreatedAt,
                x.PublishedAtUtc,
                x.LastViewedAtUtc))
            .FirstOrDefaultAsync(cancellationToken);
    }
}
