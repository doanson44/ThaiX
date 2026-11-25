namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

/// <summary>
/// An embedded glyph font from the legacy Flump export. Glyph outlines are SVG path data in
/// Flash font units (1024 units per em), which is how the original runtime scaled them.
/// </summary>
public sealed class FrontlineFlumpFont
{
    private readonly Dictionary<int, int> _glyphByCode = new();

    public FrontlineFlumpFont(
        string name,
        IReadOnlyList<string> glyphs,
        IReadOnlyList<int> codes,
        IReadOnlyList<double> advances,
        double ascent,
        double descent,
        double leading)
    {
        Name = name;
        Glyphs = glyphs;
        Advances = advances;
        Ascent = ascent;
        Descent = descent;
        Leading = leading;

        for (var index = 0; index < codes.Count && index < glyphs.Count; index++)
            _glyphByCode[codes[index]] = index;
    }

    public string Name { get; }
    public IReadOnlyList<string> Glyphs { get; }
    public IReadOnlyList<double> Advances { get; }
    public double Ascent { get; }
    public double Descent { get; }
    public double Leading { get; }

    public int IndexOf(char value) =>
        _glyphByCode.TryGetValue(value, out var index) ? index : -1;

    public string? GetGlyphPath(int glyphIndex) =>
        glyphIndex >= 0 && glyphIndex < Glyphs.Count && Glyphs[glyphIndex].Length > 0
            ? Glyphs[glyphIndex]
            : null;

    public double GetAdvance(int glyphIndex) =>
        glyphIndex >= 0 && glyphIndex < Advances.Count ? Advances[glyphIndex] : 0;
}

/// <summary>A glyph placed at a baseline position, in the text field's local space.</summary>
public sealed record FrontlineFlumpGlyphPaint(int GlyphIndex, double X, double Baseline, double Scale);

/// <summary>A pre-laid-out glyph run from a static <c>text</c> character.</summary>
public sealed record FrontlineFlumpGlyphRun(
    int FontId,
    double Size,
    byte Red,
    byte Green,
    byte Blue,
    byte Alpha,
    double X,
    double Y,
    IReadOnlyList<(int GlyphIndex, double Advance)> Glyphs);

/// <summary>
/// A text placement inside a flattened symbol: either a static <c>text</c> character or an
/// editable <c>edit</c> field whose content the caller supplies.
/// </summary>
public sealed record FrontlineFlumpTextPlacement(
    string? Name,
    bool IsField,
    FrontlineFlumpMatrix Matrix,
    double MinX,
    double MinY,
    double MaxX,
    double MaxY,
    int FontId,
    double Size,
    byte Red,
    byte Green,
    byte Blue,
    byte Alpha,
    int Align,
    double Leading,
    double LeftMargin,
    double RightMargin,
    double Indent,
    bool WordWrap,
    bool Multiline,
    bool Border,
    string Text,
    IReadOnlyList<FrontlineFlumpGlyphRun> Runs)
{
    /// <summary>Slash separated instance path inside the root symbol, for repeated field names.</summary>
    public string? Path { get; init; }

    /// <summary>The legacy field stored HTML markup that the runtime parsed into a text format.</summary>
    public bool Html { get; init; }

    public double Width => MaxX - MinX;

    public double Height => MaxY - MinY;
}

/// <summary>
/// Lays out Flow text exactly like the legacy runtime: 1024 units per em glyph scaling,
/// word wrapping against the field width, per-line alignment, and the two pixel inset the
/// original applied to editable fields.
/// </summary>
public static class FrontlineFlumpTextLayout
{
    private const double FontUnitsPerEm = 1024d;

    /// <summary>Resolves the glyphs to draw for a placement, using the caller supplied field value.</summary>
    public static IReadOnlyList<FrontlineFlumpGlyphPaint> GetPaints(
        FrontlineFlumpTextPlacement placement,
        Func<int, FrontlineFlumpFont?> fontLookup,
        string? value = null)
    {
        if (!placement.IsField)
            return StaticPaints(placement);

        var font = fontLookup(placement.FontId);
        if (font is null)
            return [];

        var text = value ?? placement.Text;
        var effective = placement;
        if (placement.Html)
        {
            var (styled, plainDefault) = ParseHtml(placement.Text, placement);
            effective = styled;
            text = value ?? plainDefault;
        }

        return FieldPaints(effective, font, text);
    }

    /// <summary>
    /// Reproduces the legacy HTML text parsing: pull the paragraph alignment, font colour and size
    /// out of the markup, then flatten the remaining tags into plain lines.
    /// </summary>
    private static (FrontlineFlumpTextPlacement Placement, string Text) ParseHtml(
        string markup,
        FrontlineFlumpTextPlacement placement)
    {
        var align = placement.Align;
        var red = placement.Red;
        var green = placement.Green;
        var blue = placement.Blue;
        var size = placement.Size;

        var alignMatch = System.Text.RegularExpressions.Regex.Match(
            markup,
            "<p[^>]*align=\"(left|right|center|justify)\"",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (alignMatch.Success)
        {
            align = alignMatch.Groups[1].Value.ToLowerInvariant() switch
            {
                "right" => 1,
                "center" => 2,
                _ => 0
            };
        }

        var colorMatch = System.Text.RegularExpressions.Regex.Match(
            markup,
            "<font[^>]*color=\"#([0-9a-f]{6})\"",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (colorMatch.Success && int.TryParse(colorMatch.Groups[1].Value, System.Globalization.NumberStyles.HexNumber, null, out var rgb))
        {
            red = (byte)((rgb >> 16) & 0xFF);
            green = (byte)((rgb >> 8) & 0xFF);
            blue = (byte)(rgb & 0xFF);
        }

        var sizeMatch = System.Text.RegularExpressions.Regex.Match(
            markup,
            "<font[^>]*size=\"(\\d+)\"",
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        if (sizeMatch.Success && int.TryParse(sizeMatch.Groups[1].Value, out var parsedSize))
            size = parsedSize;

        var plain = System.Text.RegularExpressions.Regex
            .Replace(markup, "<br\\s*/?>", "\n", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        plain = System.Text.RegularExpressions.Regex.Replace(plain, "</p>", "\n", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        plain = System.Text.RegularExpressions.Regex.Replace(plain, "<[^>]+>", string.Empty);
        plain = DecodeEntities(plain).TrimEnd('\n');

        return (placement with { Align = align, Red = red, Green = green, Blue = blue, Size = size }, plain);
    }

    /// <summary>Exposes the parsed HTML style of a field so callers can paint with its colour.</summary>
    public static FrontlineFlumpTextPlacement ApplyHtmlStyle(FrontlineFlumpTextPlacement placement) =>
        placement.Html ? ParseHtml(placement.Text, placement).Placement : placement;

    private static string DecodeEntities(string value) =>        value            .Replace("&lt;", "<", StringComparison.Ordinal)
            .Replace("&gt;", ">", StringComparison.Ordinal)
            .Replace("&amp;", "&", StringComparison.Ordinal)
            .Replace("&quot;", "\"", StringComparison.Ordinal)
            .Replace("&apos;", "'", StringComparison.Ordinal)
            .Replace("&nbsp;", " ", StringComparison.Ordinal);

    private static IReadOnlyList<FrontlineFlumpGlyphPaint> StaticPaints(FrontlineFlumpTextPlacement placement)
    {
        var paints = new List<FrontlineFlumpGlyphPaint>();
        foreach (var run in placement.Runs)
        {
            var scale = run.Size / FontUnitsPerEm;
            var x = run.X;
            foreach (var (glyphIndex, advance) in run.Glyphs)
            {
                paints.Add(new FrontlineFlumpGlyphPaint(glyphIndex, x, run.Y, scale));
                x += advance;
            }
        }

        return paints;
    }

    private static IReadOnlyList<FrontlineFlumpGlyphPaint> FieldPaints(
        FrontlineFlumpTextPlacement placement,
        FrontlineFlumpFont font,
        string text)
    {
        var paints = new List<FrontlineFlumpGlyphPaint>();
        var scale = placement.Size / FontUnitsPerEm;
        var wrapWidth = placement.Width - placement.LeftMargin - placement.RightMargin;
        var lines = LayoutLines(placement, font, text, wrapWidth);

        var ascent = font.Ascent * scale;
        var descent = font.Descent * scale;
        var lineHeight = ascent + descent + placement.Leading;

        var startX = placement.MinX + 2;
        var baseline = placement.MinY + 2 + ascent;
        var alignWidth = placement.Width - 4 - placement.LeftMargin - placement.RightMargin;

        for (var lineIndex = 0; lineIndex < lines.Count; lineIndex++)
        {
            var line = lines[lineIndex];
            var lineWidth = Measure(font, line, scale, placement.Size);
            var x = startX + placement.LeftMargin + (lineIndex == 0 ? placement.Indent : 0);

            if (placement.Align == 1)
                x += alignWidth - lineWidth;
            else if (placement.Align == 2)
                x += (alignWidth - lineWidth) / 2;

            foreach (var character in line)
            {
                var glyphIndex = font.IndexOf(character);
                if (glyphIndex < 0)
                {
                    x += placement.Size * 0.3;
                    continue;
                }

                paints.Add(new FrontlineFlumpGlyphPaint(glyphIndex, x, baseline, scale));
                x += font.GetAdvance(glyphIndex) * scale;
            }

            baseline += lineHeight;
        }

        return paints;
    }

    private static IReadOnlyList<string> LayoutLines(
        FrontlineFlumpTextPlacement placement,
        FrontlineFlumpFont font,
        string text,
        double wrapWidth)
    {
        var normalized = text.Replace("\r\n", "\n").Replace('\r', '\n');
        var source = placement.Multiline
            ? normalized.Split('\n')
            : [string.Join(' ', normalized.Split('\n'))];

        var scale = placement.Size / FontUnitsPerEm;
        var lines = new List<string>();
        foreach (var rawLine in source)
        {
            if (!placement.WordWrap)
            {
                lines.Add(rawLine);
                continue;
            }

            var words = SplitKeepingWhitespace(rawLine);
            var current = string.Empty;
            foreach (var word in words)
            {
                var candidate = current + word;
                var allowance = wrapWidth - (lines.Count == 0 ? placement.Indent : 0);
                if (current.Length > 0 && Measure(font, candidate.TrimEnd(), scale, placement.Size) > allowance)
                {
                    lines.Add(current.TrimEnd());
                    current = word.TrimStart();
                }
                else
                {
                    current = candidate;
                }
            }

            lines.Add(current.TrimEnd());
        }

        return lines;
    }

    private static IEnumerable<string> SplitKeepingWhitespace(string value)
    {
        var start = 0;
        for (var index = 0; index < value.Length; index++)
        {
            if (!char.IsWhiteSpace(value[index]))
                continue;

            var wordEnd = index;
            while (index < value.Length && char.IsWhiteSpace(value[index]))
                index++;

            if (wordEnd > start)
                yield return value[start..wordEnd];

            yield return value[wordEnd..index];
            start = index;
            index--;
        }

        if (start < value.Length)
            yield return value[start..];
    }

    private static double Measure(FrontlineFlumpFont font, string line, double scale, double size)
    {
        var width = 0d;
        foreach (var character in line)
        {
            var glyphIndex = font.IndexOf(character);
            width += glyphIndex < 0 ? size * 0.3 : font.GetAdvance(glyphIndex) * scale;
        }

        return width;
    }
}
