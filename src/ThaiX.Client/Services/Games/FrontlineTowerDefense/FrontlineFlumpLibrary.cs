using System.Text.Json;

namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

/// <summary>
/// A 2D affine transform matching the Flump <c>m</c> matrix layout:
/// <c>[a, c, tx; b, d, ty]</c>. This is the same layout used by SVG
/// <c>transform="matrix(a,b,c,d,tx,ty)"</c>.
/// </summary>
public readonly record struct FrontlineFlumpMatrix(
    double A, double B, double C, double D, double Tx, double Ty)
{
    public static readonly FrontlineFlumpMatrix Identity = new(1, 0, 0, 1, 0, 0);

    public static FrontlineFlumpMatrix Multiply(
        in FrontlineFlumpMatrix parent,
        in FrontlineFlumpMatrix child) =>
        new(
            parent.A * child.A + parent.C * child.B,
            parent.B * child.A + parent.D * child.B,
            parent.A * child.C + parent.C * child.D,
            parent.B * child.C + parent.D * child.D,
            parent.A * child.Tx + parent.C * child.Ty + parent.Tx,
            parent.B * child.Tx + parent.D * child.Ty + parent.Ty);
}

public enum FrontlineFlumpDrawKind
{
    Bitmap,
    Fill
}

/// <summary>
/// A single draw command produced by flattening a Flump character.
/// Bitmap draws reference a PNG under the legacy asset bundle's <c>img</c>
/// folder; fill draws are flat vector shapes rendered as rounded rectangles.
/// </summary>
public sealed record FrontlineFlumpDraw(
    FrontlineFlumpDrawKind Kind,
    int BitmapId,
    double Width,
    double Height,
    double MinX,
    double MinY,
    double MaxX,
    double MaxY,
    double CornerRadius,
    byte Red,
    byte Green,
    byte Blue,
    byte Alpha,
    FrontlineFlumpMatrix Matrix);

/// <summary>
/// A child placed on a sprite timeline frame, used to compose custom child
/// animation frames (turret rotation, enemy walk cycles) onto a parent symbol.
/// </summary>
public sealed record FrontlineFlumpPlacement(
    string? Name,
    int CharId,
    FrontlineFlumpMatrix Matrix);

/// <summary>
/// Minimal Flump sprite renderer for the Frontline Tower Defense legacy assets.
/// Loads the legacy <c>library.json</c>, resolves symbol names to character ids,
/// and flattens sprite timelines into bitmap/fill draw commands in local space.
/// </summary>
public sealed class FrontlineFlumpLibrary
{
    private readonly SemaphoreSlim _loadGate = new(1, 1);
    private JsonDocument? _document;
    private Dictionary<string, int> _symbolIds = new(StringComparer.Ordinal);
    private readonly Dictionary<int, string> _levelSymbols = new();
    private readonly Dictionary<int, string> _bonusSymbols = new();
    private readonly Dictionary<long, IReadOnlyList<FrontlineFlumpDraw>> _drawCache = new();
    private readonly Dictionary<long, IReadOnlyList<FrontlineFlumpPlacement>> _placementCache = new();

    /// <summary>Creates an empty library that is populated by <see cref="EnsureLoadedAsync"/>.</summary>
    public FrontlineFlumpLibrary()
    {
    }

    private FrontlineFlumpLibrary(JsonDocument document, Dictionary<string, int> symbolIds)
    {
        _document = document;
        _symbolIds = symbolIds;
        IndexLevelSymbols();
    }

    /// <summary>
    /// Loads the library JSON once from the application's own static host. The API
    /// <see cref="HttpClient"/> cannot be used here because it targets the API origin and is
    /// short-circuited in demo mode.
    /// </summary>
    public async Task EnsureLoadedAsync(string applicationBaseUri, CancellationToken cancellationToken = default)
    {
        if (_document is not null)
            return;

        await _loadGate.WaitAsync(cancellationToken);
        try
        {
            if (_document is not null)
                return;

            var libraryUri = new Uri(
                new Uri(applicationBaseUri, UriKind.Absolute),
                "games/frontline-tower-defense/assets/library.json");
            using var http = new HttpClient();
            var json = await http.GetStringAsync(libraryUri, cancellationToken);

            var parsed = Parse(json);
            _document = parsed._document;
            _symbolIds = parsed._symbolIds;
            _drawCache.Clear();
            _placementCache.Clear();
            IndexLevelSymbols();
        }
        finally
        {
            _loadGate.Release();
        }
    }

    /// <summary>Parses library JSON. Kept public for tests.</summary>
    public static FrontlineFlumpLibrary Parse(string json)
    {
        var document = JsonDocument.Parse(json);
        var symbolIds = new Dictionary<string, int>(StringComparer.Ordinal);
        if (document.RootElement.TryGetProperty("symbols", out var symbols))
        {
            foreach (var property in symbols.EnumerateObject())
                if (property.Value.ValueKind == JsonValueKind.Number && property.Value.TryGetInt32(out var id))
                    symbolIds[property.Name] = id;
        }

        return new FrontlineFlumpLibrary(document, symbolIds);
    }

    public bool IsLoaded => _document is not null;

    public int? GetSymbolId(string name) =>
        _symbolIds.TryGetValue(name, out var id) ? id : null;

    public string GetLevelSymbol(int levelNumber, bool isBonus)
    {
        var table = isBonus ? _bonusSymbols : _levelSymbols;
        return table.TryGetValue(levelNumber, out var name)
            ? name
            : string.Empty;
    }

    public int GetFrameCount(int charId)
    {
        var character = GetChar(charId);
        if (character.ValueKind != JsonValueKind.Object)
            return 1;

        if (character.TryGetProperty("n", out var n) && n.ValueKind == JsonValueKind.Number && n.TryGetInt32(out var count))
            return Math.Max(1, count);

        return 1;
    }

    /// <summary>Returns the local-space draw list for a character at a frame index (0-based).</summary>
    public IReadOnlyList<FrontlineFlumpDraw> GetDraws(int charId, int frame)
    {
        var key = Key(charId, frame);
        if (_drawCache.TryGetValue(key, out var cached))
            return cached;

        var draws = new List<FrontlineFlumpDraw>();
        Flatten(charId, frame, FrontlineFlumpMatrix.Identity, draws, 0);
        _drawCache[key] = draws;
        return draws;
    }

    /// <summary>Returns the display-list placements for a sprite frame (0-based).</summary>
    public IReadOnlyList<FrontlineFlumpPlacement> GetPlacements(int charId, int frame)
    {
        var key = Key(charId, frame);
        if (_placementCache.TryGetValue(key, out var cached))
            return cached;

        var placements = new List<FrontlineFlumpPlacement>();
        var character = GetChar(charId);
        if (character.ValueKind == JsonValueKind.Object
            && character.TryGetProperty("k", out var kind)
            && kind.GetString() == "sprite")
        {
            foreach (var placement in BuildDisplayList(charId, frame).Values)
                placements.Add(new FrontlineFlumpPlacement(placement.Name, placement.Id, placement.Matrix));
        }

        _placementCache[key] = placements;
        return placements;
    }

    private static long Key(int charId, int frame) =>
        ((long)charId << 20) | (uint)Math.Max(0, frame);

    private void IndexLevelSymbols()
    {
        _levelSymbols[1] = "Level_0001";
        foreach (var (name, id) in _symbolIds)
        {
            _ = id;
            if (name.StartsWith("FD2__fla.Level_", StringComparison.Ordinal))
            {
                var body = name["FD2__fla.Level_".Length..];
                var underscore = body.IndexOf('_');
                if (underscore <= 0)
                    continue;

                if (body.StartsWith("Bonus_", StringComparison.Ordinal))
                {
                    var bonus = body["Bonus_".Length..];
                    var bonusUnderscore = bonus.IndexOf('_');
                    if (bonusUnderscore <= 0)
                        continue;

                    if (int.TryParse(bonus[..bonusUnderscore], out var bonusIndex))
                        _bonusSymbols[bonusIndex] = name;
                }
                else if (int.TryParse(body[..underscore], out var level))
                {
                    _levelSymbols[level] = name;
                }
            }
        }
    }

    private JsonElement GetChar(int charId)
    {
        if (_document is null)
            return default;

        return _document.RootElement.TryGetProperty("chars", out var chars)
        && chars.TryGetProperty(charId.ToString(System.Globalization.CultureInfo.InvariantCulture), out var character)
            ? character
            : default;
    }

    private void Flatten(
        int charId,
        int frame,
        in FrontlineFlumpMatrix parent,
        List<FrontlineFlumpDraw> draws,
        int depth)
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
            case "bitmap":
                var width = GetDouble(character, "w");
                var height = GetDouble(character, "h");
                draws.Add(new FrontlineFlumpDraw(
                    FrontlineFlumpDrawKind.Bitmap,
                    charId,
                    width,
                    height,
                    0,
                    0,
                    width,
                    height,
                    0,
                    0,
                    0,
                    0,
                    0,
                    parent));
                return;

            case "shape":
                FlattenShape(charId, character, parent, draws);
                return;

            case "sprite":
                foreach (var placement in BuildDisplayList(charId, frame).Values)
                    Flatten(placement.Id, 0, FrontlineFlumpMatrix.Multiply(parent, placement.Matrix), draws, depth + 1);
                return;
        }
    }

    private void FlattenShape(
        int charId,
        JsonElement character,
        in FrontlineFlumpMatrix parent,
        List<FrontlineFlumpDraw> draws)
    {
        var local = ReadMatrix(character, "m") ?? FrontlineFlumpMatrix.Identity;
        var matrix = FrontlineFlumpMatrix.Multiply(parent, local);

        if (character.TryGetProperty("img", out var img) && img.ValueKind == JsonValueKind.Number && img.TryGetInt32(out var bitmapId))
        {
            var bitmap = GetChar(bitmapId);
            var width = GetDouble(bitmap, "w");
            var height = GetDouble(bitmap, "h");
            draws.Add(new FrontlineFlumpDraw(
                FrontlineFlumpDrawKind.Bitmap,
                bitmapId,
                width,
                height,
                0,
                0,
                width,
                height,
                0,
                0,
                0,
                0,
                0,
                matrix));
            return;
        }

        var (minX, minY, maxX, maxY) = ReadBounds(character);
        var (red, green, blue, alpha) = ReadFillColor(character);
        var cornerRadius = ReadCornerRadius(character, minX, maxX);

        draws.Add(new FrontlineFlumpDraw(
            FrontlineFlumpDrawKind.Fill,
            charId,
            0,
            0,
            minX,
            minY,
            maxX,
            maxY,
            cornerRadius,
            red,
            green,
            blue,
            alpha,
            matrix));
    }

    private sealed record DisplayEntry(string? Name, int Id, FrontlineFlumpMatrix Matrix);

    private Dictionary<int, DisplayEntry> BuildDisplayList(int charId, int frame)
    {
        var state = new Dictionary<int, DisplayEntry>();
        var character = GetChar(charId);
        if (character.ValueKind != JsonValueKind.Object
            || !character.TryGetProperty("frames", out var frames)
            || frames.ValueKind != JsonValueKind.Array)
            return state;

        var total = frames.GetArrayLength();
        var last = Math.Clamp(frame, 0, total - 1);
        for (var index = 0; index <= last; index++)
        {
            var frameElement = frames[index];
            if (frameElement.ValueKind != JsonValueKind.Array)
                continue;

            foreach (var operation in frameElement.EnumerateArray())
            {
                if (operation.ValueKind != JsonValueKind.Object)
                    continue;

                var op = operation.TryGetProperty("o", out var opProp)
                    ? opProp.GetString()
                    : null;
                var depth = operation.TryGetProperty("d", out var depthProp) && depthProp.TryGetInt32(out var depthValue)
                    ? depthValue
                    : 0;

                if (op == "r")
                {
                    state.Remove(depth);
                    continue;
                }

                var id = operation.TryGetProperty("id", out var idProp) && idProp.TryGetInt32(out var idValue)
                    ? idValue
                    : 0;
                var moveOnly = operation.TryGetProperty("mv", out var mvProp) && mvProp.ValueKind == JsonValueKind.Number;
                var name = operation.TryGetProperty("n", out var nameProp) && nameProp.ValueKind == JsonValueKind.String
                    ? nameProp.GetString()
                    : null;
                var matrix = ReadMatrix(operation, "m");

                if (moveOnly && state.TryGetValue(depth, out var existing))
                {
                    // A move op can also swap the instance to a different symbol
                    // (Flump reuses depth slots across frames). Keep the existing
                    // values when the op does not specify a new id/matrix/name.
                    state[depth] = new DisplayEntry(
                        name ?? existing.Name,
                        id != 0 ? id : existing.Id,
                        matrix ?? existing.Matrix);
                }
                else
                {
                    state[depth] = new DisplayEntry(name, id, matrix ?? FrontlineFlumpMatrix.Identity);
                }
            }
        }

        return state;
    }

    private static FrontlineFlumpMatrix? ReadMatrix(JsonElement element, string propertyName)
    {
        if (!element.TryGetProperty(propertyName, out var matrixElement)
            || matrixElement.ValueKind != JsonValueKind.Array)
            return null;

        var values = matrixElement.EnumerateArray().ToArray();
        if (values.Length < 6)
            return null;

        return new FrontlineFlumpMatrix(
            values[0].GetDouble(),
            values[1].GetDouble(),
            values[2].GetDouble(),
            values[3].GetDouble(),
            values[4].GetDouble(),
            values[5].GetDouble());
    }

    private static (double MinX, double MinY, double MaxX, double MaxY) ReadBounds(JsonElement element)
    {
        if (element.TryGetProperty("b", out var bounds) && bounds.ValueKind == JsonValueKind.Array)
        {
            var values = bounds.EnumerateArray().ToArray();
            if (values.Length >= 4)
                return (values[0].GetDouble(), values[1].GetDouble(), values[2].GetDouble(), values[3].GetDouble());
        }

        return (-16, -16, 16, 16);
    }

    private static (byte Red, byte Green, byte Blue, byte Alpha) ReadFillColor(JsonElement element)
    {
        if (element.TryGetProperty("f", out var fills) && fills.ValueKind == JsonValueKind.Array)
        {
            foreach (var fill in fills.EnumerateArray())
            {
                if (fill.ValueKind != JsonValueKind.Object
                    || !fill.TryGetProperty("s", out var style)
                    || style.ValueKind != JsonValueKind.Object
                    || !style.TryGetProperty("c", out var color)
                    || color.ValueKind != JsonValueKind.Array)
                    continue;

                var values = color.EnumerateArray().ToArray();
                if (values.Length >= 4)
                    return (
                        ToByte(values[0]),
                        ToByte(values[1]),
                        ToByte(values[2]),
                        ToAlpha(values[3]));
            }
        }

        return (255, 255, 255, 0);
    }

    private static byte ToByte(JsonElement element) =>
        element.ValueKind == JsonValueKind.Number
            ? (byte)Math.Clamp(Math.Round(element.GetDouble()), 0, 255)
            : (byte)0;

    private static byte ToAlpha(JsonElement element) =>
        element.ValueKind == JsonValueKind.Number
            ? (byte)Math.Clamp(Math.Round(element.GetDouble() * 255), 0, 255)
            : (byte)0;

    private static double ReadCornerRadius(JsonElement element, double minX, double maxX)
    {
        if (!element.TryGetProperty("f", out var fills) || fills.ValueKind != JsonValueKind.Array)
            return 0;

        foreach (var fill in fills.EnumerateArray())
        {
            if (fill.ValueKind != JsonValueKind.Object
                || !fill.TryGetProperty("d", out var path)
                || path.ValueKind != JsonValueKind.String)
                continue;

            var commands = path.GetString();
            if (string.IsNullOrEmpty(commands) || !commands.StartsWith('M'))
                return 0;

            var end = commands.IndexOf(' ', 1);
            if (end <= 1)
                return 0;

            if (double.TryParse(
                commands.AsSpan(1, end - 1),
                System.Globalization.NumberStyles.Float,
                System.Globalization.CultureInfo.InvariantCulture,
                out var firstX))
            {
                return Math.Max(0, (maxX - minX) / 2 - Math.Abs(firstX));
            }
        }

        return 0;
    }

    private static double GetDouble(JsonElement element, string propertyName) =>
        element.ValueKind == JsonValueKind.Object
        && element.TryGetProperty(propertyName, out var value)
        && value.ValueKind == JsonValueKind.Number
            ? value.GetDouble()
            : 0;
}
