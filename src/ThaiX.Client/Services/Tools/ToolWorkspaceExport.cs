namespace ThaiX.Client.Services.Tools;

public sealed class ToolWorkspaceExport
{
    public int Version { get; set; } = ToolCatalog.WorkspaceExportVersion;
    public string ToolSlug { get; set; } = string.Empty;
    public DateTime ExportedAtUtc { get; set; } = DateTime.UtcNow;
    public ToolWorkspaceState State { get; set; } = new();
}
