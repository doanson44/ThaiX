using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.SystemLogs.Common;

namespace ThaiX.Application.Features.SystemLogs.Queries.DownloadSystemLogFile;

public sealed record DownloadSystemLogFileQuery(string FileName)
    : IAppQuery<SystemLogFileDownloadDto?>;
