using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.PublicHtml.Commands.DeletePublicHtmlPage;

public sealed class DeletePublicHtmlPageCommandHandler(
    IApplicationDbContext dbContext,
    IFileStorage storage)
    : IRequestHandler<DeletePublicHtmlPageCommand, Guid>
{
    public async Task<Guid> Handle(DeletePublicHtmlPageCommand request, CancellationToken cancellationToken)
    {
        var page = await dbContext.PublicHtmlPages
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (page is null)
            return request.Id;

        var storageKey = page.StorageKey;
        page.SoftDeletePage();
        await dbContext.SaveChangesAsync(cancellationToken);

        await storage.DeleteAsync(storageKey, cancellationToken);
        return request.Id;
    }
}
