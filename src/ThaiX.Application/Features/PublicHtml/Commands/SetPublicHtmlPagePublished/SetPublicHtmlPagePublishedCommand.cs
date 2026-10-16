using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.PublicHtml.Commands.SetPublicHtmlPagePublished;

public sealed record SetPublicHtmlPagePublishedCommand(Guid Id, bool IsPublished) : IAppNonTransactionalCommand<Guid>;
