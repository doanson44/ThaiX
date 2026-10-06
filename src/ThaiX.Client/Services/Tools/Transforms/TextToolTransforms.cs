using System.Text;
using System.Text.RegularExpressions;
using DiffPlex;
using DiffPlex.DiffBuilder;
using DiffPlex.DiffBuilder.Model;
using Markdig;

namespace ThaiX.Client.Services.Tools.Transforms;

public static class TextToolTransforms
{
    private static readonly MarkdownPipeline MdPipeline = new MarkdownPipelineBuilder()
        .UseAdvancedExtensions()
        .Build();

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
        Markdown.ToHtml(AutoFenceBareMermaid(markdown ?? string.Empty), MdPipeline);

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

    public static string FormatSql(string sql) => SimpleIndent(sql, new[] { "select", "from", "where", "join", "left", "right", "inner", "outer", "group", "order", "having", "union", "insert", "update", "delete", "values", "set" });

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

    public static string RegexTest(string pattern, string input, string? replacement, int timeoutMs = 200)
    {
        var regex = new Regex(pattern, RegexOptions.None, TimeSpan.FromMilliseconds(timeoutMs));
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
        // Minimal: strip tags for wave deliverable; Markdig is MD→HTML. Keep reversible-ish plain text.
        var text = Regex.Replace(html ?? "", "<script[\\s\\S]*?</script>", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<style[\\s\\S]*?</style>", "", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<br\\s*/?>", "\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "</p>", "\n\n", RegexOptions.IgnoreCase);
        text = Regex.Replace(text, "<[^>]+>", "");
        return System.Net.WebUtility.HtmlDecode(text).Trim();
    }

    private static string SimpleIndent(string text, string[] breakKeywords)
    {
        var compact = Regex.Replace(text ?? "", @"\s+", " ").Trim();
        foreach (var kw in breakKeywords)
            compact = Regex.Replace(compact, $@"\b{kw}\b", "\n" + kw.ToUpperInvariant(), RegexOptions.IgnoreCase);
        return compact.Trim();
    }
}
