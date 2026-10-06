using Microsoft.Extensions.Options;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Infrastructure.Configuration;

namespace ThaiX.Infrastructure.Services;

/// <summary>
/// Generates public URLs for files stored by <see cref="IFileStorage"/>.
/// Returns relative URLs (for example: /files/images/avatar.png).
/// </summary>
public sealed class StorageFileUrlProvider : IFileUrlProvider
{
    private readonly string _baseUrl;

    public StorageFileUrlProvider(IOptions<StorageSettings> options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _baseUrl = NormalizeBaseUrl(options.Value.BaseUrl);
    }

    public string GetUrl(string storageKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storageKey);

        storageKey = NormalizeKey(storageKey);

        var encodedPath = string.Join(
            '/',
            storageKey
                .Split('/', StringSplitOptions.RemoveEmptyEntries)
                .Select(Uri.EscapeDataString));

        return $"{_baseUrl}/{encodedPath}";
    }

    private static string NormalizeBaseUrl(string? baseUrl)
    {
        if (string.IsNullOrWhiteSpace(baseUrl))
        {
            return "/files";
        }

        baseUrl = baseUrl.Trim();

        if (!baseUrl.StartsWith('/'))
        {
            baseUrl = "/" + baseUrl;
        }

        return baseUrl.TrimEnd('/');
    }

    private static string NormalizeKey(string key)
    {
        return key
            .Replace('\\', '/')
            .TrimStart('/');
    }
}
