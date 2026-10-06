using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.ApiClient;

/// <summary>
/// Represents a third-party system that can authenticate via Client Credentials flow.
/// This is NOT a human user - it's a machine-to-machine (M2M) actor.
/// </summary>
public sealed class ApiClient : BaseAuditableEntity
{
    private List<string> _scopes = new();

    /// <summary>
    /// Public client identifier (e.g., "mobile-app-v1", "partner-system-x").
    /// </summary>
    public string ClientId { get; private set; } = string.Empty;

    /// <summary>
    /// Hashed client secret (never store plaintext).
    /// </summary>
    public string ClientSecretHash { get; private set; } = string.Empty;

    /// <summary>
    /// Human-readable name for administrative purposes.
    /// </summary>
    public string Name { get; private set; } = string.Empty;

    /// <summary>
    /// Optional description of the client's purpose.
    /// </summary>
    public string? Description { get; private set; }

    /// <summary>
    /// Indicates whether this client is active and can authenticate.
    /// </summary>
    public bool IsActive { get; private set; }

    /// <summary>
    /// Scopes (permissions) granted to this client.
    /// Scopes use the same format as user permissions (Feature.Action).
    /// </summary>
    public IReadOnlyCollection<string> Scopes => _scopes.AsReadOnly();

    // Private constructor for EF Core
    private ApiClient() { }

    public static ApiClient Create(
        string clientId,
        string clientSecretHash,
        string name,
        string? description,
        IEnumerable<string> scopes,
        Guid createdBy)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            throw new ArgumentException("Client ID is required.", nameof(clientId));

        if (string.IsNullOrWhiteSpace(clientSecretHash))
            throw new ArgumentException("Client secret hash is required.", nameof(clientSecretHash));

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        if (createdBy == Guid.Empty)
            throw new ArgumentException("CreatedBy is required.", nameof(createdBy));

        var apiClient = new ApiClient
        {
            Id = Guid.NewGuid(),
            ClientId = clientId,
            ClientSecretHash = clientSecretHash,
            Name = name,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        apiClient.UpdateScopes(scopes);

        return apiClient;
    }

    public void UpdateScopes(IEnumerable<string> scopes)
    {
        _scopes = scopes
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct()
            .ToList();

        // UpdatedAt and UpdatedBy set automatically by infrastructure interceptor
    }

    public void Activate()
    {
        IsActive = true;
        // UpdatedAt and UpdatedBy set automatically by infrastructure interceptor
    }

    public void Deactivate()
    {
        IsActive = false;
        // UpdatedAt and UpdatedBy set automatically by infrastructure interceptor
    }

    public void UpdateDetails(string name, string? description)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name is required.", nameof(name));

        Name = name;
        Description = description;
        // UpdatedAt and UpdatedBy set automatically by infrastructure interceptor
    }

    public void UpdateSecret(string newSecretHash)
    {
        if (string.IsNullOrWhiteSpace(newSecretHash))
            throw new ArgumentException("Secret hash is required.", nameof(newSecretHash));

        ClientSecretHash = newSecretHash;
        // UpdatedAt and UpdatedBy set automatically by infrastructure interceptor
    }

    /// <summary>
    /// Soft deletes this API client.
    /// The client will no longer be able to authenticate.
    /// </summary>
    public void DeleteClient()
    {
        Delete(); // Inherited from BaseAuditableEntity
        // DeletedAt and DeletedBy set automatically by infrastructure interceptor
    }

    /// <summary>
    /// Restores a soft-deleted API client.
    /// </summary>
    public void RestoreClient()
    {
        Restore(); // Inherited from BaseAuditableEntity
    }
}

