namespace ThaiX.Client.Services.Tools;

public sealed class ToolStateEnvelope
{
    public int V { get; set; } = ToolCatalog.StateEnvelopeVersion;
    public DateTime SavedAtUtc { get; set; }
    public string StateJson { get; set; } = "{}";
}
