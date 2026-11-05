using Net.Codecrete.QrCodeGenerator;
using System.Security.Cryptography;
using System.Text;

namespace ThaiX.Client.Services.Tools.Transforms;

public static class GeneratorToolTransforms
{
    public static string GeneratePassword(int length, bool symbols)
    {
        length = Math.Clamp(length, 4, 128);
        const string upper = "ABCDEFGHJKLMNPQRSTUVWXYZ";
        const string lower = "abcdefghijkmnopqrstuvwxyz";
        const string digits = "23456789";
        const string syms = "!@#$%^&*-_=+?";
        var groups = symbols
            ? new[] { upper, lower, digits, syms }
            : new[] { upper, lower, digits };
        var alphabet = string.Concat(groups);
        var chars = new char[length];

        for (var i = 0; i < groups.Length; i++)
            chars[i] = groups[i][RandomNumberGenerator.GetInt32(groups[i].Length)];

        for (var i = groups.Length; i < chars.Length; i++)
            chars[i] = alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)];

        for (var i = chars.Length - 1; i > 0; i--)
        {
            var swapIndex = RandomNumberGenerator.GetInt32(i + 1);
            (chars[i], chars[swapIndex]) = (chars[swapIndex], chars[i]);
        }

        return new string(chars);
    }

    public static string PasswordStrength(string password)
    {
        var p = password ?? "";
        var score = 0;
        if (p.Length >= 8) score++;
        if (p.Length >= 12) score++;
        if (p.Any(char.IsUpper) && p.Any(char.IsLower)) score++;
        if (p.Any(char.IsDigit)) score++;
        if (p.Any(ch => !char.IsLetterOrDigit(ch))) score++;
        if (p.Length > 0 && p.Distinct().Count() <= 3) score = Math.Max(0, score - 2);
        var label = score switch
        {
            <= 1 => "Weak",
            2 => "Fair",
            3 => "Good",
            _ => "Strong"
        };
        return $"Score: {score}/5 — {label}\nLength: {p.Length}\nHeuristic only; not an entropy or breach check.";
    }

    public static string BuildCron(string minute, string hour, string dayOfMonth, string month, string dayOfWeek) =>
        $"{minute} {hour} {dayOfMonth} {month} {dayOfWeek}";

    public static string DescribeCron(string expression, DateTimeOffset? fromUtc = null)
    {
        var occurrences = GetNextCronOccurrences(expression, fromUtc ?? DateTimeOffset.UtcNow);
        var lines = occurrences.Select(value => value.ToString("yyyy-MM-dd HH:mm 'UTC'", System.Globalization.CultureInfo.InvariantCulture));
        return $"Cron: {expression}\nNext runs (UTC):\n{string.Join("\n", lines)}";
    }

    public static IReadOnlyList<DateTimeOffset> GetNextCronOccurrences(
        string expression,
        DateTimeOffset fromUtc,
        int count = 5)
    {
        if (count is < 1 or > 100)
            throw new ArgumentOutOfRangeException(nameof(count));

        var parts = (expression ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 5)
            throw new FormatException("Cron expression must contain exactly five fields.");

        var minute = ParseCronField(parts[0], 0, 59);
        var hour = ParseCronField(parts[1], 0, 23);
        var dayOfMonth = ParseCronField(parts[2], 1, 31);
        var month = ParseCronField(parts[3], 1, 12);
        var dayOfWeek = ParseCronField(parts[4], 0, 7, normalizeSunday: true);

        var utc = fromUtc.ToUniversalTime();
        var cursor = new DateTimeOffset(
            utc.Year, utc.Month, utc.Day, utc.Hour, utc.Minute, 0, TimeSpan.Zero).AddMinutes(1);
        var occurrences = new List<DateTimeOffset>(count);
        var maxMinutes = 5 * 366 * 24 * 60;

        for (var index = 0; index < maxMinutes && occurrences.Count < count; index++, cursor = cursor.AddMinutes(1))
        {
            var date = cursor.UtcDateTime;
            if (!minute.Values.Contains(date.Minute)
                || !hour.Values.Contains(date.Hour)
                || !month.Values.Contains(date.Month))
                continue;

            var matchesDay = dayOfMonth.Values.Contains(date.Day);
            var matchesWeekday = dayOfWeek.Values.Contains((int)date.DayOfWeek);
            var matchesCalendarDay = dayOfMonth.IsWildcard && dayOfWeek.IsWildcard
                || dayOfMonth.IsWildcard && matchesWeekday
                || dayOfWeek.IsWildcard && matchesDay
                || !dayOfMonth.IsWildcard && !dayOfWeek.IsWildcard && (matchesDay || matchesWeekday);

            if (matchesCalendarDay)
                occurrences.Add(cursor);
        }

        if (occurrences.Count != count)
            throw new FormatException("Cron expression has no upcoming occurrence within five years.");

        return occurrences;
    }

    private sealed record CronField(HashSet<int> Values, bool IsWildcard);

    private static CronField ParseCronField(string expression, int minimum, int maximum, bool normalizeSunday = false)
    {
        if (string.IsNullOrWhiteSpace(expression))
            throw new FormatException("Cron fields cannot be empty.");

        var values = new HashSet<int>();
        foreach (var item in expression.Split(',', StringSplitOptions.RemoveEmptyEntries))
        {
            var stepParts = item.Split('/');
            if (stepParts.Length > 2)
                throw new FormatException($"Invalid cron field: {expression}");

            var step = stepParts.Length == 2
                ? ParseCronNumber(stepParts[1], 1, maximum - minimum + 1)
                : 1;
            var range = stepParts[0];
            int start;
            int end;

            if (range == "*")
            {
                start = minimum;
                end = maximum;
            }
            else if (range.Contains('-', StringComparison.Ordinal))
            {
                var bounds = range.Split('-');
                if (bounds.Length != 2)
                    throw new FormatException($"Invalid cron range: {range}");
                start = ParseCronNumber(bounds[0], minimum, maximum);
                end = ParseCronNumber(bounds[1], minimum, maximum);
                if (start > end)
                    throw new FormatException($"Cron range start exceeds end: {range}");
            }
            else
            {
                start = ParseCronNumber(range, minimum, maximum);
                end = stepParts.Length == 2 ? maximum : start;
            }

            for (var value = start; value <= end; value += step)
                values.Add(normalizeSunday && value == 7 ? 0 : value);
        }

        if (values.Count == 0)
            throw new FormatException($"Cron field has no values: {expression}");

        return new CronField(values, expression == "*");
    }

    private static int ParseCronNumber(string value, int minimum, int maximum)
    {
        if (!int.TryParse(value, System.Globalization.NumberStyles.None,
                System.Globalization.CultureInfo.InvariantCulture, out var number)
            || number < minimum || number > maximum)
            throw new FormatException($"Cron value '{value}' must be between {minimum} and {maximum}.");

        return number;
    }

    public static string QrSvg(string text, int border = 2) =>
        QrCode.EncodeText(text ?? "", QrCode.Ecc.Medium).ToSvgString(border);

    public static string BuildCurl(string method, string url, string headers, string body)
    {
        var normalizedMethod = (method ?? string.Empty).Trim().ToUpperInvariant();
        if (normalizedMethod.Length == 0 || normalizedMethod.Any(character => character is < 'A' or > 'Z'))
            throw new FormatException("HTTP method must contain ASCII letters only.");
        if (string.IsNullOrWhiteSpace(url))
            throw new FormatException("URL is required.");

        var sb = new StringBuilder();
        sb.Append("curl -X ").Append(normalizedMethod).Append(" ");
        sb.Append(ShellQuote(url.Trim()));
        foreach (var line in (headers ?? "").Split('\n', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
        {
            var idx = line.IndexOf(':');
            if (idx <= 0) continue;
            sb.Append(" \\\n  -H ").Append(ShellQuote(line));
        }
        if (!string.IsNullOrWhiteSpace(body))
            sb.Append(" \\\n  -d ").Append(ShellQuote(body));
        return sb.ToString();
    }

    private static string ShellQuote(string value) =>
        "'" + value.Replace("'", "'\\''", StringComparison.Ordinal) + "'";

    public static string GitignorePreset(string preset) => preset.ToLowerInvariant() switch
    {
        "dotnet" => """
            ## .NET
            bin/
            obj/
            *.user
            *.suo
            .vs/
            *.nupkg
            appsettings.*.local.json
            """,
        "node" => """
            node_modules/
            dist/
            build/
            .env
            .env.*
            npm-debug.log*
            yarn-error.log*
            """,
        "vs" => """
            .vs/
            *.user
            *.suo
            *.userosscache
            *.sln.docstates
            """,
        "rider" => """
            .idea/
            *.sln.iml
            """,
        _ => "# Choose a preset: dotnet | node | vs | rider\n"
    };

    public static string ConventionalCommit(string type, string scope, string subject, bool breaking, string body)
    {
        var scopePart = string.IsNullOrWhiteSpace(scope) ? "" : $"({scope.Trim()})";
        var bang = breaking ? "!" : "";
        var sb = new StringBuilder();
        sb.Append(type.Trim().ToLowerInvariant()).Append(scopePart).Append(bang).Append(": ").Append(subject.Trim());
        if (!string.IsNullOrWhiteSpace(body))
            sb.Append("\n\n").Append(body.Trim());
        if (breaking)
            sb.Append("\n\nBREAKING CHANGE: ").Append(subject.Trim());
        return sb.ToString();
    }
}
