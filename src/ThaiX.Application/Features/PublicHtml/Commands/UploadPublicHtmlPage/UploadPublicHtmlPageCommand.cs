using MediatR;

namespace ThaiX.Application.Features.PublicHtml.Commands.UploadPublicHtmlPage;

public sealed record UploadPublicHtmlPageCommand(
    string Title,
    string Slug,
    string FileName,
    Stream Content,
    long SizeBytes) : IRequest<Guid>;
