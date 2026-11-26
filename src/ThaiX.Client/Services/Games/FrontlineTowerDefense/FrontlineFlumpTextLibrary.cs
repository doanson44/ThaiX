using System.Text.Json;

namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

/// <summary>
/// Text support for the Flump library: parses the embedded glyph fonts and flattens the
/// <c>text</c> and <c>edit</c> characters of a symbol frame so callers can draw the original
/// typography instead of browser text.
/// </summary>
public sealed partial class FrontlineFlumpLibrary
{
    private readonly Dictionary<int, FrontlineFlumpFont> _fontCache = new();
    private readonly Dictionary<long, IReadOnlyList<FrontlineFlumpTextPlacement>> _textCache = new();

    /// <summary>Returns the symbol name registered for a character id, if any.</summary>
    public string? GetSymbolName(int charId) =>
        _symbolNames.TryGetValue(charId, out var name) ? name : null;

    /// <summary>Returns an embedded glyph font, or <c>null</c> when the character is not a font.</summary>
    public FrontlineFlumpFont? GetFont(int charId)
    {
        if (_fontCache.TryGetValue(charId, out var cached))
            return cached;

        var character = GetChar(charId);
        if (character.ValueKind != JsonValueKind.Object
            || !character.TryGetProperty("k", out var kindProp)
            || kindProp.GetString() != "font")
            return null;

        var font = new FrontlineFlumpFont(
            character.TryGetProperty("name", out var nameProp) ? nameProp.GetString() ?? string.Empty : string.Empty,
            ReadStringArray(character, "glyphs"),
            ReadIntArray(character, "codes"),
            ReadDoubleArray(character, "adv"),
            GetDouble(character, "ascent"),
            GetDouble(character, "descent"),
            GetDouble(character, "leading"));

        _fontCache[charId] = font;
        return font;
    }

    /// <summary>Returns the text placements of a symbol frame in local space, including nested children.</summary>
    public IReadOnlyList<FrontlineFlumpTextPlacement> GetTextPlacements(int charId, int frame)
    {
        var key = Key(charId, frame);
        if (_textCache.TryGetValue(key, out var cached))
            return cached;

        var placements = new List<FrontlineFlumpTextPlacement>();
        FlattenText(charId, frame, null, FrontlineFlumpMatrix.Identity, placements, 0);
        _textCache[key] = placements;
        return placements;
    }

    private void FlattenText(
        int charId,
        int frame,
        string? instanceName,
        in FrontlineFlumpMatrix parent,
        List<FrontlineFlumpTextPlacement> placements,
        int depth,
        string path = "")
    {
        if (depth > 12)
            return;

        var character = GetChar(charId);
        if (character.ValueKind != JsonValueKind.Object)
            return;

        var kind = character.TryGetProperty("k", out var kindProp)
            ? kindProp.GetString()
            : null;

        switch (kind)
        {
            case "edit":
                placements.Add(ReadFieldPlacement(
                    instanceName,
                    path,
                    character,
                    FrontlineFlumpMatrix.Multiply(
                        parent,
                        ReadMatrix(character, "m") ?? FrontlineFlumpMatrix.Identity)));
                return;

            case "text":
                placements.Add(ReadStaticPlacement(
                    instanceName,
                    string.IsNullOrEmpty(path)
                        ? charId.ToString(CultureInfo.InvariantCulture)
                        : path,
                    character,
                    FrontlineFlumpMatrix.Multiply(
                        parent,
                        ReadMatrix(character, "m") ?? FrontlineFlumpMatrix.Identity)));
                return;

            case "sprite":
                foreach (var placement in BuildDisplayList(charId, frame).Values)
                {
                    var name = placement.Name ?? GetSymbolName(placement.Id);
                    FlattenText(
                        placement.Id,
                        0,
                        name,
                        FrontlineFlumpMatrix.Multiply(parent, placement.Matrix),
                        placements,
                        depth + 1,
                        string.IsNullOrEmpty(name) ? path : CombinePath(path, name));
                }

                return;
        }
    }

    private static string CombinePath(string path, string name) =>
        string.IsNullOrEmpty(path) ? name : path + "/" + name;

    private static FrontlineFlumpTextPlacement ReadFieldPlacement(
        string? instanceName,
        string path,
        JsonElement character,
        in FrontlineFlumpMatrix parent)
    {
        var bounds = ReadBounds(character);
        var color = ReadTextColor(character);
        return new FrontlineFlumpTextPlacement(
            instanceName,
            true,
            parent,
            bounds.MinX,
            bounds.MinY,
            bounds.MaxX,
            bounds.MaxY,
            GetIntProperty(character, "font"),
            character.TryGetProperty("h", out _) ? GetDouble(character, "h") : 12,
            color.Red,
            color.Green,
            color.Blue,
            color.Alpha,
            GetIntProperty(character, "align"),
            GetDouble(character, "leading"),
            GetDouble(character, "leftMargin"),
            GetDouble(character, "rightMargin"),
            GetDouble(character, "indent"),
            GetBoolProperty(character, "wordWrap"),
            GetBoolProperty(character, "multiline"),
            GetBoolProperty(character, "border"),
            character.TryGetProperty("text", out var textProp) ? textProp.GetString() ?? string.Empty : string.Empty,
            [])
        {
            Path = string.IsNullOrEmpty(path) ? instanceName : path,
            Html = GetBoolProperty(character, "html")
        };
    }

    private static FrontlineFlumpTextPlacement ReadStaticPlacement(
        string? instanceName,
        string path,
        JsonElement character,
        in FrontlineFlumpMatrix parent)
    {
        var bounds = ReadBounds(character);
        var runs = new List<FrontlineFlumpGlyphRun>();
        if (character.TryGetProperty("r", out var runsProp) && runsProp.ValueKind == JsonValueKind.Array)
        {
            foreach (var runElement in runsProp.EnumerateArray())
            {
                if (runElement.ValueKind != JsonValueKind.Object)
                    continue;

                var color = ReadTextColor(runElement);
                var glyphs = new List<(int GlyphIndex, double Advance)>();
                if (runElement.TryGetProperty("g", out var glyphsProp) && glyphsProp.ValueKind == JsonValueKind.Array)
                {
                    foreach (var glyphElement in glyphsProp.EnumerateArray())
                    {
                        var values = glyphElement.EnumerateArray().ToArray();
                        if (values.Length < 2)
                            continue;

                        glyphs.Add((values[0].GetInt32(), values[1].GetDouble()));
                    }
                }

                runs.Add(new FrontlineFlumpGlyphRun(
                    GetIntProperty(runElement, "font"),
                    GetDouble(runElement, "h"),
                    color.Red,
                    color.Green,
                    color.Blue,
                    color.Alpha,
                    GetDouble(runElement, "x"),
                    GetDouble(runElement, "y"),
                    glyphs));
            }
        }

        return new FrontlineFlumpTextPlacement(
            instanceName,
            false,
            parent,
            bounds.MinX,
            bounds.MinY,
            bounds.MaxX,
            bounds.MaxY,
            0,
            0,
            0,
            0,
            0,
            255,
            0,
            0,
            0,
            0,
            0,
            false,
            false,
            false,
            string.Empty,
            runs)
        {
            Path = string.IsNullOrEmpty(path) ? instanceName : path
        };
    }

    private static (byte Red, byte Green, byte Blue, byte Alpha) ReadTextColor(JsonElement element)
    {
        if (!element.TryGetProperty("c", out var color) || color.ValueKind != JsonValueKind.Array)
            return (0, 0, 0, 255);

        var values = color.EnumerateArray().ToArray();
        if (values.Length < 3)
            return (0, 0, 0, 255);

        var alpha = values.Length > 3 ? values[3].GetDouble() : 1d;
        return (
            (byte)Math.Clamp(values[0].GetDouble(), 0, 255),
            (byte)Math.Clamp(values[1].GetDouble(), 0, 255),
            (byte)Math.Clamp(values[2].GetDouble(), 0, 255),
            (byte)Math.Clamp(alpha * 255d, 0, 255));
    }

    private static int GetIntProperty(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind == JsonValueKind.Number
        && property.TryGetInt32(out var value)
            ? value
            : 0;

    private static bool GetBoolProperty(JsonElement element, string propertyName) =>
        element.TryGetProperty(propertyName, out var property)
        && property.ValueKind is JsonValueKind.True or JsonValueKind.False
        && property.GetBoolean();

    private static IReadOnlyList<string> ReadStringArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var array) || array.ValueKind != JsonValueKind.Array)
            return [];

        var values = new List<string>(array.GetArrayLength());
        foreach (var item in array.EnumerateArray())
            values.Add(item.ValueKind == JsonValueKind.String ? item.GetString() ?? string.Empty : string.Empty);

        return values;
    }

    private static IReadOnlyList<int> ReadIntArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var array) || array.ValueKind != JsonValueKind.Array)
            return [];

        var values = new List<int>(array.GetArrayLength());
        foreach (var item in array.EnumerateArray())
            values.Add(item.ValueKind == JsonValueKind.Number && item.TryGetInt32(out var value) ? value : 0);

        return values;
    }

    private static IReadOnlyList<double> ReadDoubleArray(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var array) || array.ValueKind != JsonValueKind.Array)
            return [];

        var values = new List<double>(array.GetArrayLength());
        foreach (var item in array.EnumerateArray())
            values.Add(item.ValueKind == JsonValueKind.Number ? item.GetDouble() : 0d);

        return values;
    }
}
