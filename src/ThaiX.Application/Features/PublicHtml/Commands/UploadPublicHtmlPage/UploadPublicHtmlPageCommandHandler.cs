using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.PublicHtml.Commands.UploadPublicHtmlPage;

public sealed class UploadPublicHtmlPageCommandHandler(IPublicHtmlPageService pages)
    : IRequestHandler<UploadPublicHtmlPageCommand, Guid>
{
    public async Task<Guid> Handle(UploadPublicHtmlPageCommand request, CancellationToken cancellationToken)
    {
        var page = await pages.UploadAsync(
            request.Title,
            request.Slug,
            request.FileName,
            request.Content,
            request.SizeBytes,
            cancellationToken);

        return page.Id;
    }
}
