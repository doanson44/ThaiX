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
        if (string.IsNullOrEmpty(csv))
            return "[]";

        var records = ParseCsvRecords(csv.TrimStart('\uFEFF'));
        if (records.Count == 0)
            return "[]";

        var headers = records[0].Select(value => value.Trim()).ToArray();
        if (headers.Any(string.IsNullOrWhiteSpace))
            throw new FormatException("CSV headers cannot be empty.");
        if (headers.Distinct(StringComparer.Ordinal).Count() != headers.Length)
            throw new FormatException("CSV headers must be unique.");

        var rows = new JsonArray();
        for (var rowIndex = 1; rowIndex < records.Count; rowIndex++)
        {
            var values = records[rowIndex];
            if (values.Count > headers.Length)
                throw new FormatException($"CSV record {rowIndex + 1} has more values than the header.");

            var obj = new JsonObject();
            for (var columnIndex = 0; columnIndex < headers.Length; columnIndex++)
                obj[headers[columnIndex]] = columnIndex < values.Count ? values[columnIndex] : string.Empty;
            rows.Add(obj);
        }

        return rows.ToJsonString(new JsonSerializerOptions { WriteIndented = true });
    }

    private static List<List<string>> ParseCsvRecords(string csv)
    {
        var records = new List<List<string>>();
        var record = new List<string>();
        var field = new StringBuilder();
        var inQuotes = false;
        var quoteClosed = false;

        for (var index = 0; index < csv.Length; index++)
        {
            var character = csv[index];

            if (inQuotes)
            {
                if (character == '"')
                {
                    if (index + 1 < csv.Length && csv[index + 1] == '"')
                    {
                        field.Append('"');
                        index++;
                    }
                    else
                    {
                        inQuotes = false;
                        quoteClosed = true;
                    }
                }
                else
                {
                    field.Append(character);
                }

                continue;
            }

            if (quoteClosed && character is not (',' or '\r' or '\n' or ' ' or '\t'))
                throw new FormatException("Unexpected character after a quoted CSV field.");

            if (character == '"')
            {
                if (field.Length > 0 || quoteClosed)
                    throw new FormatException("A quoted CSV field must begin with a quote.");

                inQuotes = true;
                continue;
            }

            if (character == ',')
            {
                record.Add(field.ToString());
                field.Clear();
                quoteClosed = false;
                continue;
            }

            if (character is '\r' or '\n')
            {
                if (character == '\r' && index + 1 < csv.Length && csv[index + 1] == '\n')
                    index++;

                record.Add(field.ToString());
                field.Clear();
                quoteClosed = false;
                if (record.Any(value => value.Length > 0) || record.Count > 1)
                    records.Add(record);
                record = [];
                continue;
            }

            if (!quoteClosed)
                field.Append(character);
        }

        if (inQuotes)
            throw new FormatException("CSV contains an unterminated quoted field.");

        if (field.Length > 0 || record.Count > 0 || quoteClosed)
        {
            record.Add(field.ToString());
            records.Add(record);
        }

        return records;
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
        EmitCsharp(doc.RootElement, ToPascal(rootName), sb, new HashSet<string>(StringComparer.Ordinal));
        return sb.ToString();
    }

    public static string JsonToTypescript(string json, string rootName = "Root")
    {
        using var doc = JsonDocument.Parse(json);
        var sb = new StringBuilder();
        EmitTs(doc.RootElement, ToPascal(rootName), sb, new HashSet<string>(StringComparer.Ordinal));
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

        if (el.ValueKind != JsonValueKind.Object || !seen.Add(name))
            return;

        var nested = new List<(JsonElement El, string TypeName)>();
        var propertyNames = new HashSet<string>(StringComparer.Ordinal);
        var nestedNames = new HashSet<string>(StringComparer.Ordinal);
        sb.AppendLine($"public class {name}");
        sb.AppendLine("{");

        foreach (var property in el.EnumerateObject())
        {
            var propertyName = UniqueName(ToPascal(property.Name), propertyNames);
            var nestedTypeName = ToPascal(property.Name);
            string typeName;

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                nestedTypeName = UniqueName(nestedTypeName, nestedNames);
                typeName = nestedTypeName + "?";
                nested.Add((property.Value, nestedTypeName));
            }
            else if (property.Value.ValueKind == JsonValueKind.Array
                && property.Value.GetArrayLength() > 0
                && property.Value[0].ValueKind == JsonValueKind.Object)
            {
                nestedTypeName = UniqueName(ToPascal(property.Name.TrimEnd('s')), nestedNames);
                typeName = $"List<{nestedTypeName}>";
                nested.Add((property.Value[0], nestedTypeName));
            }
            else
            {
                typeName = MapCsharp(property.Value, nestedTypeName);
            }

            sb.AppendLine($"    public {typeName} {CsharpIdentifier(propertyName)} {{ get; set; }}");
        }

        sb.AppendLine("}");
        sb.AppendLine();

        foreach (var item in nested)
            EmitCsharp(item.El, item.TypeName, sb, seen);
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

        if (el.ValueKind != JsonValueKind.Object || !seen.Add(name))
            return;

        var nested = new List<(JsonElement El, string TypeName)>();
        var propertyNames = new HashSet<string>(StringComparer.Ordinal);
        var nestedNames = new HashSet<string>(StringComparer.Ordinal);
        sb.AppendLine($"export interface {name} {{");

        foreach (var property in el.EnumerateObject())
        {
            var propertyName = UniqueName(property.Name, propertyNames);
            var nestedTypeName = ToPascal(property.Name);
            string typeName;

            if (property.Value.ValueKind == JsonValueKind.Object)
            {
                nestedTypeName = UniqueName(nestedTypeName, nestedNames);
                typeName = nestedTypeName;
                nested.Add((property.Value, nestedTypeName));
            }
            else if (property.Value.ValueKind == JsonValueKind.Array
                && property.Value.GetArrayLength() > 0
                && property.Value[0].ValueKind == JsonValueKind.Object)
            {
                nestedTypeName = UniqueName(ToPascal(property.Name.TrimEnd('s')), nestedNames);
                typeName = $"{nestedTypeName}[]";
                nested.Add((property.Value[0], nestedTypeName));
            }
            else
            {
                typeName = MapTs(property.Value, nestedTypeName);
            }

            sb.AppendLine($"  {TypeScriptPropertyName(propertyName)}: {typeName};");
        }

        sb.AppendLine("}");
        sb.AppendLine();

        foreach (var item in nested)
            EmitTs(item.El, item.TypeName, sb, seen);
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
        var sb = new StringBuilder();
        var capitalizeNext = true;
        foreach (var character in name ?? string.Empty)
        {
            if (!char.IsLetterOrDigit(character))
            {
                capitalizeNext = true;
                continue;
            }

            if (sb.Length == 0 && char.IsDigit(character))
                sb.Append('P');

            sb.Append(capitalizeNext ? char.ToUpperInvariant(character) : character);
            capitalizeNext = false;
        }

        return sb.Length == 0 ? "Property" : sb.ToString();
    }

    private static string UniqueName(string name, HashSet<string> usedNames)
    {
        var candidate = name;
        var suffix = 2;
        while (!usedNames.Add(candidate))
            candidate = name + suffix++;
        return candidate;
    }

    private static string CsharpIdentifier(string name)
    {
        string[] keywords =
        [
            "abstract", "as", "base", "bool", "break", "byte", "case", "catch", "char", "checked",
            "class", "const", "continue", "decimal", "default", "delegate", "do", "double", "else",
            "enum", "event", "explicit", "extern", "false", "finally", "fixed", "float", "for",
            "foreach", "goto", "if", "implicit", "in", "int", "interface", "internal", "is", "lock",
            "long", "namespace", "new", "null", "object", "operator", "out", "override", "params",
            "private", "protected", "public", "readonly", "ref", "return", "sbyte", "sealed",
            "short", "sizeof", "stackalloc", "static", "string", "struct", "switch", "this",
            "throw", "true", "try", "typeof", "uint", "ulong", "unchecked", "unsafe", "ushort",
            "using", "virtual", "void", "volatile", "while"
        ];

        return keywords.Contains(name, StringComparer.Ordinal) ? "@" + name : name;
    }

    private static string TypeScriptPropertyName(string name)
    {
        if (string.IsNullOrEmpty(name))
            return JsonSerializer.Serialize("property");

        var isDollar = (char)36;
        var validStart = char.IsLetter(name[0]) || name[0] == '_' || name[0] == isDollar;
        var validTail = name.Skip(1).All(character =>
            char.IsLetterOrDigit(character) || character == '_' || character == isDollar);

        return validStart && validTail ? name : JsonSerializer.Serialize(name);
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
