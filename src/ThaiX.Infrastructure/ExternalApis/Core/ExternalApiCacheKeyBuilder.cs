using System.Security.Cryptography;
using System.Text;

namespace ThaiX.Infrastructure.ExternalApis.Core;

/// <summary>
/// Builds deterministic cache keys for external API requests to avoid collisions.
/// Format: external:{method}:{url}:{queryHash}:{bodyHash}
/// </summary>
public sealed class ExternalApiCacheKeyBuilder
{
    private const string Prefix = "external:";

    /// <summary>
    /// Builds a cache key from the request. Query parameters are sorted before hashing.
    /// </summary>
    public string Build(ApiRequest request)
    {
        var method = request.Method.Method.ToUpperInvariant();
        var url = request.Url ?? string.Empty;
        var queryHash = ComputeHash(QueryStringBuilder.BuildQueryString(request.QueryParameters));
        var bodyHash = ComputeHash(request.Body ?? string.Empty);

        return $"{Prefix}{method}:{url}:{queryHash}:{bodyHash}";
    }

    private static string ComputeHash(string value)
    {
        if (string.IsNullOrEmpty(value))
        {
            return "empty";
        }

        var bytes = Encoding.UTF8.GetBytes(value);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
