using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.PublicHtml.Common;

namespace ThaiX.Application.Features.PublicHtml.Commands.SetPublicHtmlPagePublished;

public sealed class SetPublicHtmlPagePublishedCommandHandler(IFileStorage storage)
    : IRequestHandler<SetPublicHtmlPagePublishedCommand, Guid>
{
    public Task<Guid> Handle(SetPublicHtmlPagePublishedCommand request, CancellationToken cancellationToken) =>
        PublicHtmlPageIndex.ExecuteAsync(
            storage,
            (pages, _) =>
            {
                var index = pages.FindIndex(x => x.Id == request.Id);
                if (index < 0)
                    throw new KeyNotFoundException("HTML page was not found.");

                pages[index] = pages[index] with
                {
                    IsPublished = request.IsPublished,
                    PublishedAtUtc = request.IsPublished ? DateTime.UtcNow : null
                };
                return Task.FromResult((request.Id, true));
            },
            cancellationToken);
}
