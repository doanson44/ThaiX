namespace ThaiX.Client.Services.Http.Demo;

/// <summary>
/// Singleton flag set at startup after the API health probe.
/// </summary>
public sealed class ApiConnectivityState
{
    public required bool IsDemoMode { get; init; }

    public required string ApiBaseUrl { get; init; }

    public string? ProbeFailureReason { get; init; }
}
