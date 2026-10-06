namespace ThaiX.Application.Common.Helpers;

/// <summary>
/// Helper for master data list search with Vietnamese / accent-insensitive support.
/// Use with EF.Functions.Like and EF.Functions.Collate in query handlers.
/// </summary>
public static class MasterDataSearchHelper
{
    private const char LikeEscapeChar = '\\';

    /// <summary>
    /// Escapes a search term for use in SQL LIKE pattern with escape character.
    /// Caller should wrap result with % for contains (e.g. "%" + EscapeLikePattern(term) + "%").
    /// </summary>
    /// <param name="searchTerm">User search term (can be null or empty).</param>
    /// <returns>Escaped pattern segment, or null if searchTerm is null or whitespace.</returns>
    public static string? EscapeLikePattern(string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
            return null;

        var s = searchTerm.Trim();
        var sb = new System.Text.StringBuilder(s.Length + 4);
        foreach (var c in s)
        {
            if (c == LikeEscapeChar || c == '%' || c == '_')
                sb.Append(LikeEscapeChar);
            sb.Append(c);
        }
        return sb.ToString();
    }

    /// <summary>
    /// Builds a LIKE contains pattern: %escapedTerm%.
    /// Returns null if searchTerm is null or whitespace.
    /// </summary>
    public static string? BuildContainsPattern(string? searchTerm)
    {
        var escaped = EscapeLikePattern(searchTerm);
        return escaped is null ? null : "%" + escaped + "%";
    }

    /// <summary>
    /// Collation for accent-insensitive, case-insensitive search (e.g. Vietnamese "tim kiem" matches "tìm kiếm").
    /// Use with MariaDB / MySQL: EF.Functions.Collate(column, Latin1GeneralCiAi).
    /// </summary>
    public const string Latin1GeneralCiAi = "utf8mb4_unicode_ci";
}
