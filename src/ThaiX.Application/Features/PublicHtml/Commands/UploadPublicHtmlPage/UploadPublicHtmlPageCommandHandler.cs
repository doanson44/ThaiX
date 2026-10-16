using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.PublicHtml;

namespace ThaiX.Application.Features.PublicHtml.Commands.UploadPublicHtmlPage;

public sealed class UploadPublicHtmlPageCommandHandler(
    IApplicationDbContext dbContext,
    IFileStorage storage)
    : IRequestHandler<UploadPublicHtmlPageCommand, Guid>
{
    private const string ContentPrefix = "public-html/content";

    public async Task<Guid> Handle(UploadPublicHtmlPageCommand request, CancellationToken cancellationToken)
    {
        var slug = PublicHtmlPage.NormalizeSlug(request.Slug);
        if (await dbContext.PublicHtmlPages.AnyAsync(x => x.Slug == slug, cancellationToken))
            throw new InvalidOperationException("The slug is already in use.");

        var title = string.IsNullOrWhiteSpace(request.Title)
            ? Path.GetFileNameWithoutExtension(request.FileName)
            : request.Title.Trim();
        var storageKey = $"{ContentPrefix}/{Guid.NewGuid():N}.html";
        var contentSaved = false;

        try
        {
            storageKey = await storage.SaveAsync(request.Content, storageKey, "text/html", cancellationToken);
            contentSaved = true;

            var page = PublicHtmlPage.Create(title, slug, storageKey, request.SizeBytes);
            dbContext.PublicHtmlPages.Add(page);
            await dbContext.SaveChangesAsync(cancellationToken);
            return page.Id;
        }
        catch
        {
            if (contentSaved)
                await storage.DeleteAsync(storageKey, cancellationToken);
            throw;
        }
    }
}
