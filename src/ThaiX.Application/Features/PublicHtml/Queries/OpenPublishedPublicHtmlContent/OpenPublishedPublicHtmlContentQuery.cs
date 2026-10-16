using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.PublicHtml.Queries.OpenPublishedPublicHtmlContent;

public sealed record OpenPublishedPublicHtmlContentQuery(string Slug) : IAppQuery<Stream?>;
