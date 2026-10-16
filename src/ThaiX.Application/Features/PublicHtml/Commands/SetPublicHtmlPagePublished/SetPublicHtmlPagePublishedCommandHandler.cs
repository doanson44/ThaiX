using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.PublicHtml.Commands.SetPublicHtmlPagePublished;

public sealed class SetPublicHtmlPagePublishedCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<SetPublicHtmlPagePublishedCommand, Guid>
{
    public async Task<Guid> Handle(SetPublicHtmlPagePublishedCommand request, CancellationToken cancellationToken)
    {
        var page = await dbContext.PublicHtmlPages
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (page is null)
            throw new KeyNotFoundException("HTML page was not found.");

        if (request.IsPublished)
            page.Publish(DateTime.UtcNow);
        else
            page.Unpublish();

        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
