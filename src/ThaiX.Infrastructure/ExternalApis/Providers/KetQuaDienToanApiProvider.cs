using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text.RegularExpressions;
using ThaiX.Application.Features.ExternalData.Lottery.Queries.GetPower655Results;
using ThaiX.Infrastructure.ExternalApis.Core;
using ThaiX.Infrastructure.ExternalApis.Options;

namespace ThaiX.Infrastructure.ExternalApis.Providers;

/// <summary>
/// KetQuaDienToan external API provider for Power 6/55 lottery results.
/// Fetches HTML from ketquadientoan.com and parses the results table.
/// </summary>
public sealed partial class KetQuaDienToanApiProvider
{
    private const string ProviderName = "KetQuaDienToan";
    private const string Power655Endpoint = "Power655";

    private readonly ExternalApiService _apiService;
    private readonly ExternalApisOptions _options;
    private readonly ILogger<KetQuaDienToanApiProvider> _logger;

    public KetQuaDienToanApiProvider(
        ExternalApiService apiService,
        IOptions<ExternalApisOptions> options,
        ILogger<KetQuaDienToanApiProvider> logger)
    {
        _apiService = apiService;
        _options = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Gets the Power655 endpoint config. Throws if not configured.
    /// </summary>
    private EndpointOptions GetPower655Endpoint()
    {
        if (_options.Providers.TryGetValue(ProviderName, out var endpoints) &&
            endpoints.TryGetValue(Power655Endpoint, out var endpoint))
        {
            return endpoint;
        }

        throw new InvalidOperationException(
            $"External API endpoint '{ProviderName}:{Power655Endpoint}' is not configured.");
    }

    /// <summary>
    /// Fetches Power 6/55 lottery results for the given date range.
    /// Date format: dd-MM-yyyy.
    /// </summary>
    public async Task<IReadOnlyList<Power655ResultItem>?> GetPower655ResultsAsync(
        string dateFrom,
        string dateTo,
        CancellationToken cancellationToken = default)
    {
        var endpoint = GetPower655Endpoint();

        // Build URL with query parameters
        var url = $"{endpoint.Url}?datef={Uri.EscapeDataString(dateFrom)}&datet={Uri.EscapeDataString(dateTo)}";

        var request = new ApiRequest
        {
            Url = url,
            Method = HttpMethod.Get,
            UseCache = endpoint.UseCache,
            CacheDuration = endpoint.CacheDurationSeconds > 0
                ? TimeSpan.FromSeconds(endpoint.CacheDurationSeconds)
                : null,
            UseProxy = endpoint.UseProxy,
            TimeoutSeconds = endpoint.TimeoutSeconds > 0 ? endpoint.TimeoutSeconds : 30
        };

        var html = await _apiService.ExecuteAsync<string>(request, cancellationToken);

        if (string.IsNullOrWhiteSpace(html))
        {
            _logger.LogWarning("KetQuaDienToan Power655 returned empty response for {DateFrom} - {DateTo}",
                dateFrom, dateTo);
            return null;
        }

        return ParseResults(html);
    }

    /// <summary>
    /// Parses the HTML response to extract lottery results from the results table.
    /// </summary>
    private static IReadOnlyList<Power655ResultItem> ParseResults(string html)
    {
        var results = new List<Power655ResultItem>();

        // Match each table row: <tr> <td>Day, DD/MM/YYYY</td> <td>...(numbers)...</td> <td>JP1</td> <td>JP2</td> </tr>
        var rowPattern = RowRegex();
        var matches = rowPattern.Matches(html);

        foreach (Match match in matches)
        {
            var dayAndDate = match.Groups[1].Value; // e.g. "T3, 14/07/2026"
            var numbersCell = match.Groups[2].Value; // HTML with span elements
            var jp1Raw = match.Groups[3].Value.Trim();
            var jp2Raw = match.Groups[4].Value.Trim();

            var (dayOfWeek, drawDate) = SplitDayAndDate(dayAndDate);

            var (numbers, bonusNumber) = ExtractNumbers(numbersCell);

            if (numbers.Count != 6)
                continue;

            results.Add(new Power655ResultItem
            {
                DrawDate = drawDate,
                DayOfWeek = dayOfWeek,
                Numbers = numbers,
                BonusNumber = bonusNumber,
                Jackpot1Value = ParseJackpotValue(jp1Raw),
                Jackpot2Value = ParseJackpotValue(jp2Raw)
            });
        }

        return results;
    }

    /// <summary>
    /// Extracts the 6 main numbers and 1 bonus number from the HTML cell content.
    /// Bonus numbers are identified by the "jphu" CSS class.
    /// </summary>
    private static (List<int> Numbers, int BonusNumber) ExtractNumbers(string cellHtml)
    {
        var numbers = new List<int>();
        var bonusNumber = 0;

        var ballPattern = BallRegex();
        var ballMatches = ballPattern.Matches(cellHtml);

        foreach (Match ball in ballMatches)
        {
            var num = int.Parse(ball.Groups[1].Value, CultureInfo.InvariantCulture);
            // Check if this span has the "jphu" class (bonus ball)
            var spanStart = cellHtml.LastIndexOf('<', ball.Index);
            var spanTag = spanStart >= 0 ? cellHtml[spanStart..(cellHtml.IndexOf('>', spanStart) + 1)] : "";

            if (spanTag.Contains("jphu", StringComparison.Ordinal))
            {
                bonusNumber = num;
            }
            else if (numbers.Count < 6)
            {
                numbers.Add(num);
            }
        }

        return (numbers, bonusNumber);
    }

    /// <summary>
    /// Splits "T3, 14/07/2026" into ("T3", "14/07/2026").
    /// </summary>
    private static (string DayOfWeek, string DrawDate) SplitDayAndDate(string dayAndDate)
    {
        var commaIndex = dayAndDate.IndexOf(',');
        if (commaIndex < 0)
            return (dayAndDate, dayAndDate);

        return (dayAndDate[..commaIndex].Trim(), dayAndDate[(commaIndex + 1)..].Trim());
    }

    /// <summary>
    /// Parses a jackpot value string like "102,923,126,500" into a long.
    /// Returns 0 for empty or non-numeric values.
    /// </summary>
    private static long ParseJackpotValue(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
            return 0;

        // Remove commas and dots, then parse
        var cleaned = raw.Replace(",", "").Replace(".", "").Trim();
        return long.TryParse(cleaned, NumberStyles.None, CultureInfo.InvariantCulture, out var value) ? value : 0;
    }

    [GeneratedRegex(
    @"<tr>\s*<td>((?:T[2-7]|CN),\s*\d{2}/\d{2}/\d{4})</td>\s*<td>(.*?)</td>\s*<td>(.*?)</td>\s*<td>(.*?)</td>\s*</tr>",
    RegexOptions.Singleline | RegexOptions.Compiled)]
    private static partial Regex RowRegex();

    [GeneratedRegex(@">(\d+)<", RegexOptions.Compiled)]
    private static partial Regex BallRegex();
}
