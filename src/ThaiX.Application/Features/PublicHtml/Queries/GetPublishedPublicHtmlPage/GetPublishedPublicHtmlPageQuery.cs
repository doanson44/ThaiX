using MediatR;
using ThaiX.Application.Common.Interfaces;

namespace ThaiX.Application.Features.PublicHtml.Queries.GetPublishedPublicHtmlPage;

public sealed record GetPublishedPublicHtmlPageQuery(string Slug) : IRequest<PublicHtmlPageDto?>;
