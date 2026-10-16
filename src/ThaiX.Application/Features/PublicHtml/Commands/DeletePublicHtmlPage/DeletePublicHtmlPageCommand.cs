using MediatR;

namespace ThaiX.Application.Features.PublicHtml.Commands.DeletePublicHtmlPage;

public sealed record DeletePublicHtmlPageCommand(Guid Id) : IRequest;
