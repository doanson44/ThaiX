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

    private static string Canonical(JsonElement element)
    {
        var node = JsonNode.Parse(element.GetRawText());
        return JsonSerializer.Serialize(node is null ? null : Sort(node), Minified);
    }

    private static JsonNode Sort(JsonNode node)
    {
        if (node is JsonObject obj)
        {
            var ordered = new JsonObject();
            foreach (var property in obj.OrderBy(property => property.Key, StringComparer.Ordinal))
                ordered[property.Key] = property.Value is null ? null : Sort(property.Value.DeepClone());
            return ordered;
        }

        if (node is JsonArray array)
        {
            var next = new JsonArray();
            foreach (var item in array)
                next.Add(item is null ? null : Sort(item.DeepClone()));
            return next;
        }

        return node.DeepClone();
    }

    private static void DiffNodes(
        JsonNode? left,
        JsonNode? right,
        string path,
        StringBuilder output,
        bool hasLeft = true,
        bool hasRight = true)
    {
        if (!hasLeft || !hasRight)
        {
            output.AppendLine($"{path}: value mismatch");
            output.AppendLine($"  left:  {(hasLeft ? FormatNode(left) : "<missing>")}");
            output.AppendLine($"  right: {(hasRight ? FormatNode(right) : "<missing>")}");
            return;
        }

        if (left is null && right is null)
            return;

        if (left is null || right is null || left.GetType() != right.GetType())
        {
            output.AppendLine($"{path}: type/value mismatch");
            output.AppendLine($"  left:  {FormatNode(left)}");
            output.AppendLine($"  right: {FormatNode(right)}");
            return;
        }

        if (left is JsonValue)
        {
            if (!string.Equals(left.ToJsonString(), right!.ToJsonString(), StringComparison.Ordinal))
            {
                output.AppendLine($"{path}:");
                output.AppendLine($"  left:  {FormatNode(left)}");
                output.AppendLine($"  right: {FormatNode(right)}");
            }

            return;
        }

        if (left is JsonObject leftObject && right is JsonObject rightObject)
        {
            foreach (var key in leftObject.Select(property => property.Key)
                         .Union(rightObject.Select(property => property.Key), StringComparer.Ordinal)
                         .OrderBy(key => key, StringComparer.Ordinal))
            {
                var hasLeftProperty = leftObject.ContainsKey(key);
                var hasRightProperty = rightObject.ContainsKey(key);
                DiffNodes(
                    hasLeftProperty ? leftObject[key] : null,
                    hasRightProperty ? rightObject[key] : null,
                    AppendPropertyPath(path, key),
                    output,
                    hasLeftProperty,
                    hasRightProperty);
            }

            return;
        }

        if (left is JsonArray leftArray && right is JsonArray rightArray)
        {
            var length = Math.Max(leftArray.Count, rightArray.Count);
            for (var index = 0; index < length; index++)
            {
                var hasLeftItem = index < leftArray.Count;
                var hasRightItem = index < rightArray.Count;
                DiffNodes(
                    hasLeftItem ? leftArray[index] : null,
                    hasRightItem ? rightArray[index] : null,
                    $"{path}[{index}]",
                    output,
                    hasLeftItem,
                    hasRightItem);
            }
        }
    }

    private static string AppendPropertyPath(string path, string propertyName)
    {
        var isIdentifier = propertyName.Length > 0
            && (char.IsLetter(propertyName[0]) || propertyName[0] == '_')
            && propertyName.Skip(1).All(character => char.IsLetterOrDigit(character) || character == '_');

        return isIdentifier
            ? $"{path}.{propertyName}"
            : $"{path}[{JsonSerializer.Serialize(propertyName)}]";
    }

    private static string FormatNode(JsonNode? node) => node?.ToJsonString() ?? "null";

}
