using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.PublicHtml.Common;

namespace ThaiX.Application.Features.PublicHtml.Queries.GetPublicHtmlPages;

public sealed record GetPublicHtmlPagesQuery : IAppQuery<IReadOnlyCollection<PublicHtmlPageDto>>;