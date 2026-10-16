using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.PublicHtml.Commands.DeletePublicHtmlPage;

public sealed class DeletePublicHtmlPageCommandHandler(IPublicHtmlPageService pages)
    : IRequestHandler<DeletePublicHtmlPageCommand>
{
    public Task Handle(DeletePublicHtmlPageCommand request, CancellationToken cancellationToken) =>
        pages.DeleteAsync(request.Id, cancellationToken);
}
