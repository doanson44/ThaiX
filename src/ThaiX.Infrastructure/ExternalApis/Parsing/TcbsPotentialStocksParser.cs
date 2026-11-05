using System.Text.RegularExpressions;

namespace ThaiX.Infrastructure.ExternalApis.Parsing;

/// <summary>
/// Extracts ticker and research PDF link pairs from the rendered TCBS potential stock page.
/// Each item on that page links to https://static.tcbs.com.vn/oneclick/{TICKER}.pdf.
/// </summary>
public static partial class TcbsPotentialStocksParser
{
    [GeneratedRegex(@"https?://static\.tcbs\.com\.vn/oneclick/(?<ticker>[A-Z]{3})\.pdf", RegexOptions.IgnoreCase)]
    private static partial Regex PdfLinkRegex();

    /// <summary>Parses the page HTML into distinct, ticker-sorted entries.</summary>
    public static IReadOnlyList<TcbsPotentialStock> Parse(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return [];
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var result = new List<TcbsPotentialStock>();

        foreach (Match match in PdfLinkRegex().Matches(html))
        {
            var ticker = match.Groups["ticker"].Value.ToUpperInvariant();
            if (!seen.Add(ticker))
            {
                continue;
            }

            result.Add(new TcbsPotentialStock(ticker, NormalizeUrl(match.Value)));
        }

        return result.OrderBy(x => x.Ticker, StringComparer.Ordinal).ToList();
    }

    private static string NormalizeUrl(string url)
    {
        return url.StartsWith("http://", StringComparison.OrdinalIgnoreCase)
            ? string.Concat("https://", url.AsSpan("http://".Length))
            : url;
    }
}

/// <summary>A ticker listed on the potential stock page with its research PDF link.</summary>
public sealed record TcbsPotentialStock(string Ticker, string Url);
