using ThaiX.Application.Features.PublicHtml.Common;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.PublicHtml.Queries.GetPublishedPublicHtmlPage;

public sealed record GetPublishedPublicHtmlPageQuery(string Slug) : IAppQuery<PublicHtmlPageDto?>;
