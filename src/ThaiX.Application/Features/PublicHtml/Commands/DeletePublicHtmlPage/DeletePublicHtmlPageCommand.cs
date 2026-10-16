using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.PublicHtml.Commands.DeletePublicHtmlPage;

public sealed record DeletePublicHtmlPageCommand(Guid Id) : IAppNonTransactionalCommand<Guid>;
