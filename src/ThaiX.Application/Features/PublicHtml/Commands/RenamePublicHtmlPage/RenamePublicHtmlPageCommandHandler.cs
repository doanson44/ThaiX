using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Aggregates.PublicHtml;

namespace ThaiX.Application.Features.PublicHtml.Commands.RenamePublicHtmlPage;

public sealed class RenamePublicHtmlPageCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<RenamePublicHtmlPageCommand, Guid>
{
    public async Task<Guid> Handle(RenamePublicHtmlPageCommand request, CancellationToken cancellationToken)
    {
        var slug = PublicHtmlPage.NormalizeSlug(request.Slug);
        var page = await dbContext.PublicHtmlPages
            .Include(x => x.Slugs)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (page is null)
            throw new KeyNotFoundException("HTML page was not found.");

        if (await dbContext.PublicHtmlPageSlugs.AnyAsync(
            x => x.PublicHtmlPageId != request.Id && x.Slug == slug,
            cancellationToken))
            throw new InvalidOperationException("The slug is already in use.");

        page.RenameSlug(slug);
        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
