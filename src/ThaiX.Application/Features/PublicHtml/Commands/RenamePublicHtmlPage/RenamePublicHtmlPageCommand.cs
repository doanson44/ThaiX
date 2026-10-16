using MediatR;

namespace ThaiX.Application.Features.PublicHtml.Commands.RenamePublicHtmlPage;

public sealed record RenamePublicHtmlPageCommand(Guid Id, string Slug) : IRequest<Guid>;
