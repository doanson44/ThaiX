using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;

namespace ThaiX.Client.Services.Tools.Transforms;

public static class JsonToolTransforms
{
    private static readonly JsonSerializerOptions Pretty = new() { WriteIndented = true };
    private static readonly JsonSerializerOptions Minified = new() { WriteIndented = false };

    public static string PrettyPrint(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(doc.RootElement, Pretty);
    }

    public static string Minify(string json)
    {
        using var doc = JsonDocument.Parse(json);
        return JsonSerializer.Serialize(doc.RootElement, Minified);
    }

    public static void Validate(string json) => JsonDocument.Parse(json);

    public static string Compare(string left, string right)
    {
        using var a = JsonDocument.Parse(left);
        using var b = JsonDocument.Parse(right);
        var sa = Canonical(a.RootElement);
        var sb = Canonical(b.RootElement);
        if (sa == sb) return "Documents are equal (canonical form).";

        var diffs = new StringBuilder();
        DiffNodes(JsonNode.Parse(sa), JsonNode.Parse(sb), "$", diffs);
        return diffs.Length == 0
            ? "Documents differ after canonicalize; textual forms differ."
            : diffs.ToString();
    }

    private static string Canonical(JsonElement el) =>
        JsonSerializer.Serialize(Sort(JsonNode.Parse(el.GetRawText())!), Minified);

    private static JsonNode Sort(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            var ordered = new JsonObject();
            foreach (var prop in obj.OrderBy(p => p.Key, StringComparer.Ordinal))
                ordered[prop.Key] = prop.Value is null ? null : Sort(prop.Value.DeepClone());
            return ordered;
        }

        if (node is JsonArray arr)
        {
            var next = new JsonArray();
            foreach (var item in arr)
                next.Add(item is null ? null : Sort(item.DeepClone()));
            return next;
        }

        return node.DeepClone();
    }

    private static void DiffNodes(JsonNode? a, JsonNode? b, string path, StringBuilder sb)
    {
        if (a is null && b is null) return;
        if (a is null || b is null || a.GetType() != b.GetType())
        {
            sb.AppendLine($"{path}: type/value mismatch");
            sb.AppendLine($"  left:  {a}");
            sb.AppendLine($"  right: {b}");
            return;
        }

        if (a is JsonValue)
        {
            if (!string.Equals(a.ToJsonString(), b.ToJsonString(), StringComparison.Ordinal))
            {
                sb.AppendLine($"{path}:");
                sb.AppendLine($"  left:  {a}");
                sb.AppendLine($"  right: {b}");
            }
            return;
        }

        if (a is JsonObject ao && b is JsonObject bo)
        {
            foreach (var key in ao.Select(p => p.Key).Union(bo.Select(p => p.Key)).OrderBy(k => k))
                DiffNodes(ao[key], bo[key], $"{path}.{key}", sb);
            return;
        }

        if (a is JsonArray aa && b is JsonArray ba)
        {
            var len = Math.Max(aa.Count, ba.Count);
            for (var i = 0; i < len; i++)
                DiffNodes(i < aa.Count ? aa[i] : null, i < ba.Count ? ba[i] : null, $"{path}[{i}]", sb);
        }
    }
}
