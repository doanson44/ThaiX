using MediatR;

namespace ThaiX.Application.Features.PublicHtml.Commands.SetPublicHtmlPagePublished;

public sealed record SetPublicHtmlPagePublishedCommand(Guid Id, bool IsPublished) : IRequest<Guid>;
