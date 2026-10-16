using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.PublicHtml.Queries.GetPublicHtmlPages;

public sealed record GetPublicHtmlPagesQuery : IRequest<IReadOnlyCollection<PublicHtmlPageDto>>;
