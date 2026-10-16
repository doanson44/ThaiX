using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.PublicHtml.Commands.RecordPublicHtmlPageView;

public sealed record RecordPublicHtmlPageViewCommand(Guid Id) : IAppNonTransactionalCommand<Guid>;
