using MediatR;

namespace ThaiX.Application.Features.PublicHtml.Queries.OpenPublishedPublicHtmlContent;

public sealed record OpenPublishedPublicHtmlContentQuery(string Slug) : IRequest<Stream?>;
