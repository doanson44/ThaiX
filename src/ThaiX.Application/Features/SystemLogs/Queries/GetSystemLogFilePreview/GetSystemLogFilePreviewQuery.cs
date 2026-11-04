using ThaiX.Application.Common.Interfaces.Messaging;
using ThaiX.Application.Features.SystemLogs.Common;

namespace ThaiX.Application.Features.SystemLogs.Queries.GetSystemLogFilePreview;

public sealed record GetSystemLogFilePreviewQuery(string FileName)
    : IAppQuery<SystemLogFilePreviewDto?>;
