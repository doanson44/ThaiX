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

    public static string DescribeCron(string expression) =>
        $"Cron: {expression}\n(Preview only — validate in your scheduler.)";

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
