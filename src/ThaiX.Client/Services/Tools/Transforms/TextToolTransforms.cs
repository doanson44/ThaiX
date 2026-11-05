using DiffPlex;
using Ganss.Xss;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Markdig;
using System.Text;
using System.Text.RegularExpressions;

namespace ThaiX.Client.Services.Tools.Transforms;

public static class TextToolTransforms
{
    private static readonly MarkdownPipeline MdPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .DisableHtml()
        .Build();

    private static readonly HtmlSanitizer PreviewSanitizer = new();

    private static readonly Regex MermaidStartLine = new(
        @"^\s*(erDiagram|flowchart|graph|sequenceDiagram|classDiagram|stateDiagram(?:-v2)?|pie|gantt|mindmap|timeline|journey)\b",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex MermaidKeywordLine = new(
        @"^\s*(?:subgraph\b|end\b|direction\b|classDef\b|style\b|linkStyle\b|click\b|%%)",
        RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.Compiled);

    private static readonly Regex MermaidNodeLine = new(
        @"^\s*[\w][\w-]*\s*(?:\[+|\]|\(+|\)+|\{+|\}+)",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);

    public static string MarkdownToHtml(string markdown) =>
        PreviewSanitizer.Sanitize(Markdown.ToHtml(AutoFenceBareMermaid(markdown ?? string.Empty), MdPipeline));

    /// <summary>
    /// Bare Mermaid mid-document (after titles/lists) is plain text to Markdig — wrap each block in ```mermaid.
    /// </summary>
    internal static string AutoFenceBareMermaid(string markdown)
    {
        var text = markdown.Replace("\r\n", "\n");
        if (string.IsNullOrWhiteSpace(text)) return text;

        var lines = text.Split('\n');
        var output = new StringBuilder(text.Length + 64);
        var insideFence = false;

        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmedStart = line.TrimStart();

            if (trimmedStart.StartsWith("```", StringComparison.Ordinal))
            {
                insideFence = !insideFence;
                AppendLine(output, line, i == lines.Length - 1);
                continue;
            }

            if (!insideFence && MermaidStartLine.IsMatch(line))
            {
                var block = new List<string> { line };
                var j = i + 1;
                while (j < lines.Length)
                {
                    var next = lines[j];
                    if (next.TrimStart().StartsWith("```", StringComparison.Ordinal))
                        break;

                    if (string.IsNullOrWhiteSpace(next))
                    {
                        var k = j + 1;
                        while (k < lines.Length && string.IsNullOrWhiteSpace(lines[k]))
                            k++;
                        if (k >= lines.Length)
                            break;
                        if (!LooksLikeMermaidBody(lines[k]))
                            break;
                        while (j < k)
                            block.Add(lines[j++]);
                        continue;
                    }

                    if (!LooksLikeMermaidBody(next))
                        break;

                    block.Add(next);
                    j++;
                }

                output.AppendLine("```mermaid");
                foreach (var bl in block)
                    output.AppendLine(bl);
                output.Append("```");
                if (j < lines.Length)
                    output.AppendLine();

                i = j - 1;
                continue;
            }

            AppendLine(output, line, i == lines.Length - 1);
        }

        return output.ToString();
    }

    private static bool LooksLikeMermaidBody(string line)
    {
        var trimmed = line.Trim();
        // Markdown thematic breaks must not extend an auto-fenced Mermaid block.
        if (trimmed is "---" or "***" or "___" || Regex.IsMatch(trimmed, @"^-{3,}$|^\*{3,}$|^_{3,}$"))
            return false;

        return MermaidStartLine.IsMatch(line)
            || MermaidKeywordLine.IsMatch(line)
            || MermaidNodeLine.IsMatch(line)
            || line.Contains("-->", StringComparison.Ordinal)
            || Regex.IsMatch(line, @"\w\s+---\s+\w")
            || line.Contains("-.-", StringComparison.Ordinal)
            || line.Contains("==>", StringComparison.Ordinal)
            || line.Contains("->>", StringComparison.Ordinal)
            || line.Contains("-->>", StringComparison.Ordinal)
            || Regex.IsMatch(trimmed, @"^(participant|actor|alt|else|opt|loop|par|and|critical|break|rect|activate|deactivate|Note|autonumber)\b", RegexOptions.IgnoreCase);
    }

    private static void AppendLine(StringBuilder sb, string line, bool isLast)
    {
        if (isLast) sb.Append(line);
        else sb.AppendLine(line);
    }

    public static string FormatSql(string sql)
    {
        var text = sql ?? string.Empty;
        var keywords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "SELECT", "FROM", "WHERE", "JOIN", "LEFT", "RIGHT", "INNER", "OUTER",
            "GROUP", "ORDER", "HAVING", "UNION", "INSERT", "UPDATE", "DELETE",
            "VALUES", "SET", "ON", "LIMIT", "OFFSET", "RETURNING"
        };
        var output = new StringBuilder(text.Length + 32);
        var pendingSpace = false;
        var quote = '\0';
        var lineComment = false;
        var blockComment = false;

        for (var index = 0; index < text.Length; index++)
        {
            var character = text[index];
            var next = index + 1 < text.Length ? text[index + 1] : '\0';

            if (lineComment)
            {
                output.Append(character);
                if (character == '\n') lineComment = false;
                continue;
            }

            if (blockComment)
            {
                output.Append(character);
                if (character == '*' && next == '/')
                {
                    output.Append(next);
                    index++;
                    blockComment = false;
                }
                continue;
            }

            if (quote != '\0')
            {
                output.Append(character);
                var closingCharacter = quote == '[' ? ']' : quote;
                if (character == closingCharacter)
                {
                    if (next == closingCharacter)
                    {
                        output.Append(next);
                        index++;
                    }
                    else
                    {
                        quote = '\0';
                    }
                }
                continue;
            }

            if (character == '-' && next == '-')
            {
                AppendPendingSpace(output, ref pendingSpace);
                output.Append("--");
                index++;
                lineComment = true;
                continue;
            }

            if (character == '/' && next == '*')
            {
                AppendPendingSpace(output, ref pendingSpace);
                output.Append("/*");
                index++;
                blockComment = true;
                continue;
            }

            if (character == '\'' || character == '"' || character == '[' || character == (char)96)
            {
                AppendPendingSpace(output, ref pendingSpace);
                quote = character;
                output.Append(character);
                continue;
            }

            if (char.IsWhiteSpace(character))
            {
                pendingSpace = true;
                continue;
            }

            if (char.IsLetter(character) || character == '_')
            {
                var wordStart = index;
                while (index + 1 < text.Length && (char.IsLetterOrDigit(text[index + 1]) || text[index + 1] == '_'))
                    index++;

                var word = text[wordStart..(index + 1)];
                if (keywords.Contains(word))
                {
                    TrimTrailingSpaces(output);
                    if (output.Length > 0 && output[^1] != '\n')
                        output.AppendLine();
                    output.Append(word.ToUpperInvariant());
                }
                else
                {
                    AppendPendingSpace(output, ref pendingSpace);
                    output.Append(word);
                }

                continue;
            }

            AppendPendingSpace(output, ref pendingSpace);
            output.Append(character);
            if (character == ';')
            {
                output.AppendLine();
                pendingSpace = false;
            }
        }

        return output.ToString().Trim();
    }

    private static void AppendPendingSpace(StringBuilder output, ref bool pendingSpace)
    {
        if (pendingSpace && output.Length > 0 && output[^1] != '\n' && output[^1] != ' ')
            output.Append(' ');
        pendingSpace = false;
    }

    private static void TrimTrailingSpaces(StringBuilder output)
    {
        while (output.Length > 0 && output[^1] == ' ')
            output.Length--;
    }

    public static string Diff(string left, string right)
    {
        var builder = new InlineDiffBuilder(new Differ());
        var model = builder.BuildDiffModel(left ?? "", right ?? "");
        var sb = new StringBuilder();
        foreach (var line in model.Lines)
        {
            var prefix = line.Type switch
            {
                ChangeType.Inserted => "+",
                ChangeType.Deleted => "-",
                ChangeType.Modified => "~",
                _ => " "
            };
            sb.Append(prefix).Append(' ').AppendLine(line.Text);
        }
        return sb.ToString();
    }

    public static string RegexTest(
        string pattern,
        string input,
        string? replacement,
        int timeoutMs = 200,
        bool ignoreCase = false)
    {
        var options = RegexOptions.CultureInvariant;
        if (ignoreCase)
            options |= RegexOptions.IgnoreCase;
        var regex = new Regex(pattern, options, TimeSpan.FromMilliseconds(timeoutMs));
        var matches = regex.Matches(input ?? "");
        var sb = new StringBuilder();
        sb.AppendLine($"Matches: {matches.Count}");
        var i = 0;
        foreach (Match m in matches)
        {
            sb.AppendLine($"[{i}] index={m.Index} length={m.Length} value={m.Value}");
            for (var g = 1; g < m.Groups.Count; g++)
                sb.AppendLine($"  group {g}: {m.Groups[g].Value}");
            i++;
        }

        if (replacement is not null)
        {
            sb.AppendLine();
            sb.AppendLine("Replace:");
            sb.AppendLine(regex.Replace(input ?? "", replacement));
        }

        return sb.ToString();
    }

    public static string HtmlToMarkdownRough(string html)
    {
        var text = Regex.Replace(html ?? string.Empty, "<(script|style)\\b[^>]*>[\\s\\S]*?</\\1\\s*>", string.Empty, RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<!--[\\s\\S]*?-->", string.Empty);
        text = Regex.Replace(text, "<h([1-6])\\b[^>]*>([\\s\\S]*?)</h\\1\\s*>", match =>
            $"{new string('#', int.Parse(match.Groups[1].Value, System.Globalization.CultureInfo.InvariantCulture))} {match.Groups[2].Value.Trim()}\n\n",
            RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<a\\b[^>]*href=[\\\"']([^\\\"']+)[\\\"'][^>]*>([\\s\\S]*?)</a>", "[$2]($1)", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<(strong|b)\\b[^>]*>([\\s\\S]*?)</\\1\\s*>", "**$2**", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<(em|i)\\b[^>]*>([\\s\\S]*?)</\\1\\s*>", "*$2*", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<code\\b[^>]*>([\\s\\S]*?)</code>", "\u0060$1\u0060", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<br\\s*/?>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<li\\b[^>]*>", "- ", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "</li>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "</(p|div|section|ul|ol|blockquote|pre)>", "\n\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<(p|div|section|ul|ol|blockquote|pre)\\b[^>]*>", string.Empty, RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<[^>]+>", string.Empty);
        text = System.Net.WebUtility.HtmlDecode(text).Replace("\r\n", "\n");
        text = Regex.Replace(text, @"[ \t]+\n", "\n");
        text = Regex.Replace(text, @"\n{3,}", "\n\n");
        return text.Trim();
    }


}
