using System.Text;

namespace ThaiX.Infrastructure.ExternalApis.Core;

/// <summary>
/// Builds query strings safely: encoding, single ?, no duplicate separators.
/// </summary>
public static class QueryStringBuilder
{
    /// <summary>
    /// Appends query parameters to a base URL. Adds ? only if needed; encodes keys and values.
    /// </summary>
    /// <param name="baseUrl">URL without query string (or with existing query).</param>
    /// <param name="parameters">Query parameters; keys and values are encoded.</param>
    /// <returns>Full URL with query string, or baseUrl if parameters is null/empty.</returns>
    public static string AppendQueryString(string baseUrl, IReadOnlyDictionary<string, string>? parameters)
    {
        if (parameters == null || parameters.Count == 0)
        {
            return baseUrl;
        }

        var query = BuildQueryString(parameters);
        if (string.IsNullOrEmpty(query))
        {
            return baseUrl;
        }

        var separator = baseUrl.Contains('?') ? "&" : "?";
        return baseUrl.TrimEnd('&', '?') + separator + query;
    }

    /// <summary>
    /// Builds a query string from parameters. Keys sorted alphabetically; keys and values encoded.
    /// </summary>
    public static string BuildQueryString(IReadOnlyDictionary<string, string>? parameters)
    {
        if (parameters == null || parameters.Count == 0)
        {
            return string.Empty;
        }

        var builder = new StringBuilder();
        foreach (var kvp in parameters.OrderBy(kvp => kvp.Key, StringComparer.Ordinal))
        {
            if (builder.Length > 0)
            {
                builder.Append('&');
            }

            builder.Append(Uri.EscapeDataString(kvp.Key));
            builder.Append('=');
            builder.Append(Uri.EscapeDataString(kvp.Value ?? string.Empty));
        }

        return builder.ToString();
    }
}
