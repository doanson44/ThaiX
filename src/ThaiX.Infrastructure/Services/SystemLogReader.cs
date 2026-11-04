using System.Globalization;
using System.Text;
using Microsoft.Extensions.Configuration;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.SystemLogs.Common;

namespace ThaiX.Infrastructure.Services;

public sealed class SystemLogReader : ISystemLogReader
{
    private const int PreviewCharacterLimit = 100_000;

    private readonly string _directoryPath;
    private readonly string _fileNamePrefix;
    private readonly string _extension;

    public SystemLogReader(IConfiguration configuration)
    {
        var fileSink = configuration.GetSection("Serilog:WriteTo")
            .GetChildren()
            .FirstOrDefault(section =>
                string.Equals(section["Name"], "File", StringComparison.OrdinalIgnoreCase));

        var configuredPath = fileSink?["Args:path"];
        if (string.IsNullOrWhiteSpace(configuredPath))
        {
            throw new InvalidOperationException("The Serilog file sink path is not configured.");
        }

        var fullPath = Path.GetFullPath(configuredPath, Directory.GetCurrentDirectory());
        _directoryPath = Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory();
        _fileNamePrefix = Path.GetFileNameWithoutExtension(fullPath);
        _extension = Path.GetExtension(fullPath);
    }

    public Task<IReadOnlyCollection<SystemLogFileDto>> GetFilesAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!Directory.Exists(_directoryPath))
        {
            return Task.FromResult<IReadOnlyCollection<SystemLogFileDto>>([]);
        }

        var files = Directory.EnumerateFiles(
                _directoryPath,
                $"{_fileNamePrefix}*{_extension}",
                SearchOption.TopDirectoryOnly)
            .Select(path => new FileInfo(path))
            .Select(file => TryGetLogDate(file.Name, out var date)
                ? new SystemLogFileDto(file.Name, date, file.Length, file.LastWriteTimeUtc)
                : null)
            .Where(file => file is not null)
            .Cast<SystemLogFileDto>()
            .OrderByDescending(file => file.Date)
            .ToArray();

        return Task.FromResult<IReadOnlyCollection<SystemLogFileDto>>(files);
    }

    public async Task<SystemLogFilePreviewDto?> ReadPreviewAsync(
        string fileName,
        CancellationToken cancellationToken)
    {
        var path = ResolveFilePath(fileName);
        if (path is null)
        {
            return null;
        }

        await using var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: 4096,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        var buffer = new char[PreviewCharacterLimit];
        var totalRead = 0;

        while (totalRead < buffer.Length)
        {
            var read = await reader.ReadAsync(buffer.AsMemory(totalRead), cancellationToken);
            if (read == 0)
            {
                break;
            }

            totalRead += read;
        }

        var isTruncated = await reader.ReadAsync(new char[1].AsMemory(), cancellationToken) > 0;

        return new SystemLogFilePreviewDto(
            fileName,
            new string(buffer, 0, totalRead),
            isTruncated);
    }

    public Task<SystemLogFileDownloadDto?> OpenReadAsync(
        string fileName,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var path = ResolveFilePath(fileName);
        if (path is null)
        {
            return Task.FromResult<SystemLogFileDownloadDto?>(null);
        }

        var stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite | FileShare.Delete,
            bufferSize: 81920,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        return Task.FromResult<SystemLogFileDownloadDto?>(
            new SystemLogFileDownloadDto(fileName, stream));
    }

    private string? ResolveFilePath(string fileName)
    {
        if (string.IsNullOrWhiteSpace(fileName) ||
            !string.Equals(Path.GetFileName(fileName), fileName, StringComparison.Ordinal) ||
            !TryGetLogDate(fileName, out _))
        {
            return null;
        }

        var path = Path.Combine(_directoryPath, fileName);
        return File.Exists(path) ? path : null;
    }

    private bool TryGetLogDate(string fileName, out DateOnly date)
    {
        date = default;
        var nameWithoutExtension = Path.GetFileNameWithoutExtension(fileName);

        if (!nameWithoutExtension.StartsWith(_fileNamePrefix, StringComparison.Ordinal))
        {
            return false;
        }

        var datePart = nameWithoutExtension[_fileNamePrefix.Length..];
        return DateOnly.TryParseExact(
            datePart,
            "yyyyMMdd",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out date);
    }
}
