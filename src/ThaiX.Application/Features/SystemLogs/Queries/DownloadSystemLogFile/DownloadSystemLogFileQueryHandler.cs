using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.SystemLogs.Common;

namespace ThaiX.Application.Features.SystemLogs.Queries.DownloadSystemLogFile;

public sealed class DownloadSystemLogFileQueryHandler(ISystemLogReader logReader)
    : IRequestHandler<DownloadSystemLogFileQuery, SystemLogFileDownloadDto?>
{
    public Task<SystemLogFileDownloadDto?> Handle(
        DownloadSystemLogFileQuery request,
        CancellationToken cancellationToken) =>
        logReader.OpenReadAsync(request.FileName, cancellationToken);
}
