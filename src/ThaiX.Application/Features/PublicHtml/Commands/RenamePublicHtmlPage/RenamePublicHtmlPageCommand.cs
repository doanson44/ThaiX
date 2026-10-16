using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.PublicHtml.Commands.RenamePublicHtmlPage;

public sealed record RenamePublicHtmlPageCommand(Guid Id, string Slug) : IAppNonTransactionalCommand<Guid>;
