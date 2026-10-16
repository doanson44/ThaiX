using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.PublicHtml.Commands.RenamePublicHtmlPage;

public sealed class RenamePublicHtmlPageCommandHandler(IPublicHtmlPageService pages)
    : IRequestHandler<RenamePublicHtmlPageCommand, Guid>
{
    public async Task<Guid> Handle(RenamePublicHtmlPageCommand request, CancellationToken cancellationToken)
    {
        var page = await pages.RenameAsync(request.Id, request.Slug, cancellationToken);
        return page.Id;
    }
}
