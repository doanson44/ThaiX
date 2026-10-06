using System.Globalization;
using System.Text;

namespace ThaiX.Client.Services.Http.Demo;

/// <summary>
/// Offline-safe placeholder images for demo mode (SVG data URIs — no external CDN).
/// </summary>
internal static class DemoImageUrls
{
    private static readonly string[] Palette =
    [
        "#0f172a", "#1e293b", "#334155", "#0ea5e9", "#f59e0b",
        "#10b981", "#8b5cf6", "#ef4444", "#14b8a6", "#eab308"
    ];

    /// <summary>Circular-style avatar (square SVG with initials / seed label).</summary>
    public static string Avatar(string seed, int size = 128)
    {
        var (bg, fg) = ColorsFor(seed);
        var label = Initials(seed);
        return Svg(size, size, bg, fg, label, rounded: true);
    }

    /// <summary>Landscape featured / cover image.</summary>
    public static string Featured(string seed, int width = 800, int height = 450)
    {
        var (bg, fg) = ColorsFor(seed);
        var label = Truncate(seed, 18);
        return Svg(width, height, bg, fg, label, rounded: false);
    }

    /// <summary>Square logo / coin icon.</summary>
    public static string Logo(string seed, int size = 96)
    {
        var (bg, fg) = ColorsFor(seed);
        var label = Truncate(seed, 6).ToUpperInvariant();
        return Svg(size, size, bg, fg, label, rounded: false);
    }

    /// <summary>Chart / portfolio thumbnail for TCBS-style previews.</summary>
    public static string Chart(string seed, int width = 320, int height = 200)
    {
        var (bg, fg) = ColorsFor(seed);
        var accent = Palette[(Math.Abs(seed.GetHashCode()) + 3) % Palette.Length];
        var svg =
            $"<svg xmlns='http://www.w3.org/2000/svg' width='{width}' height='{height}' viewBox='0 0 {width} {height}'>" +
            $"<rect width='100%' height='100%' fill='{bg}'/>" +
            $"<polyline fill='none' stroke='{accent}' stroke-width='3' points='{Polyline(seed, width, height)}'/>" +
            $"<text x='50%' y='88%' text-anchor='middle' fill='{fg}' font-family='Segoe UI,Arial,sans-serif' font-size='14'>Demo chart</text>" +
            "</svg>";
        return ToDataUri(svg);
    }

    private static string Svg(int width, int height, string bg, string fg, string label, bool rounded)
    {
        var rx = rounded ? Math.Min(width, height) / 2 : 8;
        var fontSize = Math.Max(12, Math.Min(width, height) / 4);
        var svg =
            $"<svg xmlns='http://www.w3.org/2000/svg' width='{width}' height='{height}' viewBox='0 0 {width} {height}'>" +
            $"<rect width='100%' height='100%' rx='{rx}' ry='{rx}' fill='{bg}'/>" +
            $"<text x='50%' y='54%' text-anchor='middle' dominant-baseline='middle' fill='{fg}' " +
            $"font-family='Segoe UI,Arial,sans-serif' font-size='{fontSize.ToString(CultureInfo.InvariantCulture)}' font-weight='600'>{Escape(label)}</text>" +
            "</svg>";
        return ToDataUri(svg);
    }

    private static string ToDataUri(string svg) =>
        "data:image/svg+xml;utf8," + Uri.EscapeDataString(svg);

    private static (string Bg, string Fg) ColorsFor(string seed)
    {
        var h = Math.Abs(seed.GetHashCode());
        var bg = Palette[h % Palette.Length];
        var fg = (h / Palette.Length) % 2 == 0 ? "#f8fafc" : "#fbbf24";
        return (bg, fg);
    }

    private static string Initials(string seed)
    {
        var parts = seed.Split([' ', '-', '_', '.', '@'], StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length >= 2)
        {
            return $"{char.ToUpperInvariant(parts[0][0])}{char.ToUpperInvariant(parts[1][0])}";
        }

        var s = Truncate(seed, 2).ToUpperInvariant();
        return string.IsNullOrEmpty(s) ? "DM" : s;
    }

    private static string Truncate(string value, int max)
    {
        if (string.IsNullOrWhiteSpace(value)) return "Demo";
        value = value.Trim();
        return value.Length <= max ? value : value[..max];
    }

    private static string Escape(string value) =>
        new StringBuilder(value)
            .Replace("&", "&amp;")
            .Replace("<", "&lt;")
            .Replace(">", "&gt;")
            .Replace("\"", "&quot;")
            .ToString();

    private static string Polyline(string seed, int width, int height)
    {
        var rnd = new Random(seed.GetHashCode());
        var points = new StringBuilder();
        var mid = height * 0.55;
        for (var i = 0; i <= 8; i++)
        {
            var x = (int)(i / 8.0 * (width - 24) + 12);
            var y = (int)(mid + (rnd.NextDouble() - 0.5) * height * 0.45);
            if (i > 0) points.Append(' ');
            points.Append(x).Append(',').Append(y);
        }

        return points.ToString();
    }
}
