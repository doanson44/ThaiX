using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.PublicHtml.Common;

namespace ThaiX.Application.Features.PublicHtml.Commands.DeletePublicHtmlPage;

public sealed class DeletePublicHtmlPageCommandHandler(IFileStorage storage)
    : IRequestHandler<DeletePublicHtmlPageCommand, Guid>
{
    public async Task<Guid> Handle(DeletePublicHtmlPageCommand request, CancellationToken cancellationToken)
    {
        var storageKey = await PublicHtmlPageIndex.ExecuteAsync(
            storage,
            (pages, _) =>
            {
                var page = pages.FirstOrDefault(x => x.Id == request.Id);
                if (page is null)
                    return Task.FromResult(((string?)null, false));

                pages.Remove(page);
                return Task.FromResult(((string?)page.StorageKey, true));
            },
            cancellationToken);

        if (storageKey is not null)
            await storage.DeleteAsync(storageKey, cancellationToken);

        return request.Id;
    }
}
