namespace ThaiX.Client.Services.Tools;

public interface IToolStateStore
{
    /// <summary>
    /// Loads the tool draft. Signed-in users with cloud access read their JSONBin first;
    /// anonymous or local-only tools use browser storage.
    /// </summary>
    Task<ToolStateEnvelope?> LoadAsync(ToolDefinition tool, CancellationToken cancellationToken = default);

    /// <summary>
    /// Persists the tool draft using the active storage policy (cloud when eligible, otherwise browser).
    /// </summary>
    Task SaveLocalAsync(ToolDefinition tool, string stateJson, CancellationToken cancellationToken = default);

    /// <summary>
    /// Clears the draft from browser storage and the user's JSONBin when cloud persistence applies.
    /// </summary>
    Task ClearLocalAsync(ToolDefinition tool, CancellationToken cancellationToken = default);

    Task SaveCloudAsync(ToolDefinition tool, string stateJson, string displayName, CancellationToken cancellationToken = default);

    /// <summary>Upserts cloud state and returns the public share URL (4h TTL).</summary>
    Task<string> ShareAsync(ToolDefinition tool, string stateJson, string displayName, CancellationToken cancellationToken = default);

    Task<bool> CanCloudWriteAsync();
}
