using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.SystemLogs.Common;

namespace ThaiX.Application.Features.SystemLogs.Queries.GetSystemLogFiles;

public sealed record GetSystemLogFilesQuery : IAppQuery<IReadOnlyCollection<SystemLogFileDto>>;
