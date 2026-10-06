using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using ThaiX.Application.Common.Constants;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Infrastructure.Configuration;

namespace ThaiX.Infrastructure.Services;

/// <summary>
/// Local filesystem implementation of <see cref="IFileStorage"/>.
/// Designed to be replaceable with S3/Azure Blob without changing the Application layer.
/// </summary>
public sealed class LocalFileStorage : IFileStorage
{
    private readonly string _rootPath;
    private readonly ILogger<LocalFileStorage> _logger;

    public LocalFileStorage(
        IWebHostEnvironment environment,
        IOptions<StorageSettings> options,
        ILogger<LocalFileStorage> logger)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(options);

        _logger = logger ?? throw new ArgumentNullException(nameof(logger));

        _rootPath = Path.GetFullPath(
            Path.Combine(environment.ContentRootPath, options.Value.RootPath));

        Directory.CreateDirectory(_rootPath);
    }

    public async Task<string> SaveAsync(
        Stream stream,
        string key,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(stream);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        string path = ResolvePath(key);

        Directory.CreateDirectory(Path.GetDirectoryName(path)!);

        string tempFile = $"{path}.{Guid.NewGuid():N}.tmp";

        try
        {
            await using (var file = new FileStream(
                tempFile,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 81920,
                options: FileOptions.Asynchronous | FileOptions.SequentialScan))
            {
                await stream.CopyToAsync(file, cancellationToken);
                await file.FlushAsync(cancellationToken);
            }

            File.Move(tempFile, path, overwrite: true);

            _logger.LogDebug("Stored file '{StorageKey}'.", key);

            return NormalizeKey(key);
        }
        catch
        {
            if (File.Exists(tempFile))
            {
                try
                {
                    File.Delete(tempFile);
                }
                catch
                {
                    // Ignore cleanup failure.
                }
            }

            throw;
        }
    }

    public Task<Stream> GetAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        string path = ResolvePath(key);

        if (!File.Exists(path))
        {
            throw new OperationFailedException(ErrorCodes.FILE_NOT_FOUND, "The requested file was not found.");
        }

        Stream stream = new FileStream(
            path,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 81920,
            options: FileOptions.Asynchronous | FileOptions.SequentialScan);

        return Task.FromResult(stream);
    }

    public Task DeleteAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(key);

        string path = ResolvePath(key);

        if (File.Exists(path))
        {
            File.Delete(path);
            _logger.LogDebug("Deleted file '{StorageKey}'.", key);
        }

        return Task.CompletedTask;
    }

    private string ResolvePath(string key)
    {
        key = NormalizeKey(key);

        string fullPath = Path.GetFullPath(
            Path.Combine(
                _rootPath,
                key.Replace('/', Path.DirectorySeparatorChar)));

        if (!fullPath.StartsWith(_rootPath, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Invalid storage key.");
        }

        return fullPath;
    }

    private static string NormalizeKey(string key)
    {
        return key
            .Replace('\\', '/')
            .TrimStart('/');
    }
}