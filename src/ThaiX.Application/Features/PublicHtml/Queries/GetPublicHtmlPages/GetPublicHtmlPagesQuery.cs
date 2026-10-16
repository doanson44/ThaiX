using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Common.Interfaces.Messaging;

namespace ThaiX.Application.Features.PublicHtml.Queries.GetPublicHtmlPages;

public sealed record GetPublicHtmlPagesQuery : IAppQuery<IReadOnlyCollection<PublicHtmlPageDto>>;
