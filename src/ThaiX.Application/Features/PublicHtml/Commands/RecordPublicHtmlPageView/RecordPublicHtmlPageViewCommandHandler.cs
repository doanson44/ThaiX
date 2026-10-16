using MediatR;
using Microsoft.EntityFrameworkCore;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.PublicHtml.Commands.RecordPublicHtmlPageView;

public sealed class RecordPublicHtmlPageViewCommandHandler(IApplicationDbContext dbContext)
    : IRequestHandler<RecordPublicHtmlPageViewCommand, Guid>
{
    public async Task<Guid> Handle(
        RecordPublicHtmlPageViewCommand request,
        CancellationToken cancellationToken)
    {
        var page = await dbContext.PublicHtmlPages
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (page is null)
            return request.Id;

        page.RecordView(DateTime.UtcNow);
        await dbContext.SaveChangesAsync(cancellationToken);
        return request.Id;
    }
}
