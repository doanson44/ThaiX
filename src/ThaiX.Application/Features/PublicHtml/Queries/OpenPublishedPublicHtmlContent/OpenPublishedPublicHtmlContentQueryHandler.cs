using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.PublicHtml;

namespace ThaiX.Application.Features.PublicHtml.Queries.OpenPublishedPublicHtmlContent;

public sealed class OpenPublishedPublicHtmlContentQueryHandler(
    IApplicationDbContext dbContext,
    IFileStorage storage)
    : IRequestHandler<OpenPublishedPublicHtmlContentQuery, Stream?>
{
    public async Task<Stream?> Handle(
        OpenPublishedPublicHtmlContentQuery request,
        CancellationToken cancellationToken)
    {
        string slug;
        try { slug = PublicHtmlPage.NormalizeSlug(request.Slug); }
        catch (ArgumentException) { return null; }

        var storageKey = await dbContext.PublicHtmlPages
            .AsNoTracking()
            .Where(x => x.IsPublished && x.Slugs.Any(s => s.Slug == slug))
            .Select(x => x.StorageKey)
            .FirstOrDefaultAsync(cancellationToken);

        if (storageKey is null)
            return null;

        try { return await storage.GetAsync(storageKey, cancellationToken); }
        catch (OperationFailedException) { return null; }
    }
}
