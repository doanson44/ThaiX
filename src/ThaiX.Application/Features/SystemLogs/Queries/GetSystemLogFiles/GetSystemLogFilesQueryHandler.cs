using MediatR;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.SystemLogs.Common;

namespace ThaiX.Application.Features.SystemLogs.Queries.GetSystemLogFiles;

public sealed class GetSystemLogFilesQueryHandler(ISystemLogReader logReader)
    : IRequestHandler<GetSystemLogFilesQuery, IReadOnlyCollection<SystemLogFileDto>>
{
    public Task<IReadOnlyCollection<SystemLogFileDto>> Handle(
        GetSystemLogFilesQuery request,
        CancellationToken cancellationToken) =>
        logReader.GetFilesAsync(cancellationToken);
}
