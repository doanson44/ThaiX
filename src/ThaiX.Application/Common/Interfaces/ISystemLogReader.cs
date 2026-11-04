using ThaiX.Application.Features.SystemLogs.Common;

namespace ThaiX.Application.Common.Interfaces;

public interface ISystemLogReader
{
    Task<IReadOnlyCollection<SystemLogFileDto>> GetFilesAsync(CancellationToken cancellationToken);

    Task<SystemLogFilePreviewDto?> ReadPreviewAsync(string fileName, CancellationToken cancellationToken);

    Task<SystemLogFileDownloadDto?> OpenReadAsync(string fileName, CancellationToken cancellationToken);
}
