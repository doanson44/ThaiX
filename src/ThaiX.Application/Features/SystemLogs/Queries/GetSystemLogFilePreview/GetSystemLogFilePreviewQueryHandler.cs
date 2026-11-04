using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.SystemLogs.Common;

namespace ThaiX.Application.Features.SystemLogs.Queries.GetSystemLogFilePreview;

public sealed class GetSystemLogFilePreviewQueryHandler(ISystemLogReader logReader)
    : IRequestHandler<GetSystemLogFilePreviewQuery, SystemLogFilePreviewDto?>
{
    public Task<SystemLogFilePreviewDto?> Handle(
        GetSystemLogFilePreviewQuery request,
        CancellationToken cancellationToken) =>
        logReader.ReadPreviewAsync(request.FileName, cancellationToken);
}
