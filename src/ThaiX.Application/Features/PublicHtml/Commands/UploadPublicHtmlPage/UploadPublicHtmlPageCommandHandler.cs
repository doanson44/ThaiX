using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.PublicHtml.Common;

namespace ThaiX.Application.Features.PublicHtml.Commands.UploadPublicHtmlPage;

public sealed class UploadPublicHtmlPageCommandHandler(IFileStorage storage)
    : IRequestHandler<UploadPublicHtmlPageCommand, Guid>
{
    private const string ContentPrefix = "public-html/content";

    public async Task<Guid> Handle(UploadPublicHtmlPageCommand request, CancellationToken cancellationToken)
    {
        var id = Guid.NewGuid();
        var storageKey = $"{ContentPrefix}/{id:N}.html";
        var contentSaved = false;

        try
        {
            return await PublicHtmlPageIndex.ExecuteAsync(
                storage,
                async (pages, ct) =>
                {
                    var slug = request.Slug.Trim().ToLowerInvariant();
                    if (pages.Any(x => x.Slug.Equals(slug, StringComparison.OrdinalIgnoreCase) ||
                        x.PreviousSlugs.Contains(slug, StringComparer.OrdinalIgnoreCase)))
                        throw new InvalidOperationException("The slug is already in use.");

                    var title = string.IsNullOrWhiteSpace(request.Title)
                        ? Path.GetFileNameWithoutExtension(request.FileName)
                        : request.Title.Trim();

                    await storage.SaveAsync(request.Content, storageKey, "text/html", ct);
                    contentSaved = true;
                    pages.Add(new StoredPublicHtmlPage(
                        id, title, slug, [], storageKey, request.SizeBytes, false, DateTime.UtcNow, null));
                    return (id, true);
                },
                cancellationToken);
        }
        catch
        {
            if (contentSaved)
                await storage.DeleteAsync(storageKey, cancellationToken);
            throw;
        }
    }
}
