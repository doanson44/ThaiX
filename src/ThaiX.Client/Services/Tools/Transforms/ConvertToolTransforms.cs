using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Xml;
using System.Xml.Linq;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

namespace ThaiX.Client.Services.Tools.Transforms;

public static class ConvertToolTransforms
{
    public static string ValidateYaml(string yaml)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .Build();
        _ = deserializer.Deserialize<object>(yaml);
        return "YAML is valid.";
    }

    public static string YamlToJson(string yaml)
    {
        var deserializer = new DeserializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .Build();
        var obj = deserializer.Deserialize<object>(yaml);
        return JsonSerializer.Serialize(obj, new JsonSerializerOptions { WriteIndented = true });
    }

    public static string JsonToYaml(string json)
    {
        using var doc = JsonDocument.Parse(json);
        var obj = JsonSerializer.Deserialize<object>(doc.RootElement.GetRawText());
        var serializer = new SerializerBuilder()
            .WithNamingConvention(NullNamingConvention.Instance)
            .Build();
        return serializer.Serialize(obj);
    }

    public static string FormatXml(string xml)
    {
        var doc = XDocument.Parse(xml);
        var sb = new StringBuilder();
        var settings = new XmlWriterSettings
        {
            Indent = true,
            OmitXmlDeclaration = doc.Declaration is null,
            NewLineOnAttributes = false
        };
        // Dispose/flush writer before reading StringBuilder — otherwise output is empty.
        using (var writer = XmlWriter.Create(sb, settings))
            doc.Save(writer);
        return sb.ToString();
    }

    public static string CsvToJson(string csv)
    {
        var lines = csv.Replace("\r\n", "\n").Split('\n', StringSplitOptions.RemoveEmptyEntries);
        if (lines.Length == 0) return "[]";
        var headers = SplitCsvLine(lines[0]);
        var rows = new JsonArray();
        for (var i = 1; i < lines.Length; i++)
        {
            var cols = SplitCsvLine(lines[i]);
            var obj = new JsonObject();
            for (var c = 0; c < headers.Count; c++)
                obj[headers[c]] = c < cols.Count ? cols[c] : "";
            rows.Add(obj);
        }
        return rows.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    public static string JsonToCsv(string json)
    {
        using var doc = JsonDocument.Parse(json);
        if (doc.RootElement.ValueKind != JsonValueKind.Array)
            throw new FormatException("JSON root must be an array of objects.");

        var rows = doc.RootElement.EnumerateArray().ToList();
        if (rows.Count == 0) return string.Empty;

        var headers = new List<string>();
        foreach (var row in rows)
        {
            if (row.ValueKind != JsonValueKind.Object)
                throw new FormatException("Each array item must be an object.");
            foreach (var p in row.EnumerateObject())
            {
                if (!headers.Contains(p.Name, StringComparer.Ordinal))
                    headers.Add(p.Name);
            }
        }

        var sb = new StringBuilder();
        sb.AppendLine(string.Join(',', headers.Select(EscapeCsv)));
        foreach (var row in rows)
        {
            var cells = headers.Select(h =>
                row.TryGetProperty(h, out var v) ? EscapeCsv(JsonValueToString(v)) : "");
            sb.AppendLine(string.Join(',', cells));
        }
        return sb.ToString();
    }

    public static string JsonToCsharp(string json, string rootName = "Root")
    {
        using var doc = JsonDocument.Parse(json);
        var sb = new StringBuilder();
        EmitCsharp(doc.RootElement, rootName, sb, new HashSet<string>(StringComparer.Ordinal));
        return sb.ToString();
    }

    public static string JsonToTypescript(string json, string rootName = "Root")
    {
        using var doc = JsonDocument.Parse(json);
        var sb = new StringBuilder();
        EmitTs(doc.RootElement, rootName, sb, new HashSet<string>(StringComparer.Ordinal));
        return sb.ToString();
    }

    private static void EmitCsharp(JsonElement el, string name, StringBuilder sb, HashSet<string> seen)
    {
        if (el.ValueKind == JsonValueKind.Array)
        {
            var item = el.GetArrayLength() > 0 ? el[0] : default;
            var itemName = name.EndsWith("s", StringComparison.Ordinal) ? name[..^1] : name + "Item";
            if (item.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                EmitCsharp(item, itemName, sb, seen);
            sb.AppendLine($"// Use List<{MapCsharp(item, itemName)}> for root array.");
            return;
        }

        if (el.ValueKind != JsonValueKind.Object) return;
        if (!seen.Add(name)) return;

        var nested = new List<(string Prop, JsonElement El, string TypeName)>();
        sb.AppendLine($"public class {name}");
        sb.AppendLine("{");
        foreach (var p in el.EnumerateObject())
        {
            var typeName = MapCsharp(p.Value, ToPascal(p.Name));
            if (p.Value.ValueKind == JsonValueKind.Object)
                nested.Add((p.Name, p.Value, ToPascal(p.Name)));
            else if (p.Value.ValueKind == JsonValueKind.Array && p.Value.GetArrayLength() > 0 && p.Value[0].ValueKind == JsonValueKind.Object)
                nested.Add((p.Name, p.Value[0], ToPascal(p.Name.TrimEnd('s'))));

            sb.AppendLine($"    public {typeName} {ToPascal(p.Name)} {{ get; set; }}");
        }
        sb.AppendLine("}");
        sb.AppendLine();
        foreach (var n in nested)
            EmitCsharp(n.El, n.TypeName, sb, seen);
    }

    private static void EmitTs(JsonElement el, string name, StringBuilder sb, HashSet<string> seen)
    {
        if (el.ValueKind == JsonValueKind.Array)
        {
            var item = el.GetArrayLength() > 0 ? el[0] : default;
            var itemName = name.EndsWith("s", StringComparison.Ordinal) ? name[..^1] : name + "Item";
            if (item.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
                EmitTs(item, itemName, sb, seen);
            sb.AppendLine($"export type {name} = {MapTs(item, itemName)}[];");
            return;
        }

        if (el.ValueKind != JsonValueKind.Object) return;
        if (!seen.Add(name)) return;

        var nested = new List<(JsonElement El, string TypeName)>();
        sb.AppendLine($"export interface {name} {{");
        foreach (var p in el.EnumerateObject())
        {
            var typeName = MapTs(p.Value, ToPascal(p.Name));
            if (p.Value.ValueKind == JsonValueKind.Object)
                nested.Add((p.Value, ToPascal(p.Name)));
            else if (p.Value.ValueKind == JsonValueKind.Array && p.Value.GetArrayLength() > 0 && p.Value[0].ValueKind == JsonValueKind.Object)
                nested.Add((p.Value[0], ToPascal(p.Name.TrimEnd('s'))));
            sb.AppendLine($"  {p.Name}: {typeName};");
        }
        sb.AppendLine("}");
        sb.AppendLine();
        foreach (var n in nested)
            EmitTs(n.El, n.TypeName, sb, seen);
    }

    private static string MapCsharp(JsonElement el, string objectName) => el.ValueKind switch
    {
        JsonValueKind.String => "string?",
        JsonValueKind.Number => el.TryGetInt64(out _) ? "long" : "double",
        JsonValueKind.True or JsonValueKind.False => "bool",
        JsonValueKind.Null => "object?",
        JsonValueKind.Object => objectName + "?",
        JsonValueKind.Array when el.GetArrayLength() > 0 => $"List<{MapCsharp(el[0], objectName)}>",
        JsonValueKind.Array => "List<object>?",
        _ => "object?"
    };

    private static string MapTs(JsonElement el, string objectName) => el.ValueKind switch
    {
        JsonValueKind.String => "string",
        JsonValueKind.Number => "number",
        JsonValueKind.True or JsonValueKind.False => "boolean",
        JsonValueKind.Null => "null",
        JsonValueKind.Object => objectName,
        JsonValueKind.Array when el.GetArrayLength() > 0 => $"{MapTs(el[0], objectName)}[]",
        JsonValueKind.Array => "unknown[]",
        _ => "unknown"
    };

    private static string ToPascal(string name)
    {
        if (string.IsNullOrWhiteSpace(name)) return "Property";
        var parts = name.Split([' ', '-', '_', '.'], StringSplitOptions.RemoveEmptyEntries);
        var sb = new StringBuilder();
        foreach (var p in parts)
            sb.Append(char.ToUpperInvariant(p[0])).Append(p.AsSpan(1));
        var s = sb.ToString();
        return char.IsLetter(s[0]) ? s : "P" + s;
    }

    private static List<string> SplitCsvLine(string line)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;
        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                {
                    sb.Append('"');
                    i++;
                }
                else inQuotes = !inQuotes;
            }
            else if (ch == ',' && !inQuotes)
            {
                result.Add(sb.ToString());
                sb.Clear();
            }
            else sb.Append(ch);
        }
        result.Add(sb.ToString());
        return result;
    }

    private static string EscapeCsv(string value)
    {
        if (value.Contains('"') || value.Contains(',') || value.Contains('\n'))
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        return value;
    }

    private static string JsonValueToString(JsonElement v) => v.ValueKind switch
    {
        JsonValueKind.String => v.GetString() ?? "",
        JsonValueKind.Null => "",
        _ => v.ToString()
    };
}
