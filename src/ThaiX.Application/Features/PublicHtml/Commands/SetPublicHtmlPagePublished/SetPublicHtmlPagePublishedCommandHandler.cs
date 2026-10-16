using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.PublicHtml.Commands.SetPublicHtmlPagePublished;

public sealed class SetPublicHtmlPagePublishedCommandHandler(IPublicHtmlPageStore pages)
    : IRequestHandler<SetPublicHtmlPagePublishedCommand, Guid>
{
    public async Task<Guid> Handle(SetPublicHtmlPagePublishedCommand request, CancellationToken cancellationToken)
    {
        var page = await pages.SetPublishedAsync(request.Id, request.IsPublished, cancellationToken);
        return page.Id;
    }
}
