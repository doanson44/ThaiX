namespace ThaiX.Client.Services.Tools;

public enum ToolCategory
{
    Developer = 0,
    Trading = 1,
    Risk = 2,
    Crypto = 3
}

public enum ToolKind
{
    TextTransform = 0,
    Calculator = 1
}

public sealed record ToolDefinition(
    string Slug,
    ToolCategory Category,
    ToolKind Kind,
    string TitleKey,
    string DescriptionKey,
    string Icon,
    bool AllowCloudSave,
    bool AllowShare);
