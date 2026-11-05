namespace ThaiX.Client.Services.Tools;

/// <summary>Shared persisted shape for ToolHost (covers text tools and calculator field bags).</summary>
public sealed class ToolWorkspaceState
{
    public string Input { get; set; } = string.Empty;
    public string Input2 { get; set; } = string.Empty;
    public string Output { get; set; } = string.Empty;
    public string Option { get; set; } = string.Empty;
    public bool Flag { get; set; }
    public int Number { get; set; } = 16;
    public Dictionary<string, string> Fields { get; set; } = new(StringComparer.OrdinalIgnoreCase);
}
