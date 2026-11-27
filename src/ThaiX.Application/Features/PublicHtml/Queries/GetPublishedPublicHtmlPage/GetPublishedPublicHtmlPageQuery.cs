using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.PublicHtml.Common;

namespace ThaiX.Application.Features.PublicHtml.Queries.GetPublishedPublicHtmlPage;

public sealed record GetPublishedPublicHtmlPageQuery(string Slug) : IAppQuery<PublicHtmlPageDto?>;