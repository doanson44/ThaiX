using Microsoft.EntityFrameworkCore;
using ThaiX.Domain.Aggregates.ApiClient;
using ThaiX.Infrastructure.Persistence;
using ThaiX.Infrastructure.Security;

namespace ThaiX.Infrastructure.Identity;

/// <summary>
/// Service for managing API clients (M2M authentication).
/// </summary>
public sealed class ApiClientService
{
    private readonly ApplicationDbContext _context;

    public ApiClientService(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new API client.
    /// Returns the plaintext secret (only time it will be visible).
    /// </summary>
    public async Task<(ApiClient Client, string PlaintextSecret)> CreateAsync(
        string clientId,
        string name,
        string? description,
        IEnumerable<string> scopes,
        Guid createdBy,
        CancellationToken cancellationToken = default)
    {
        // Check if client ID already exists
        var exists = await _context.ApiClients
            .AnyAsync(c => c.ClientId == clientId, cancellationToken);

        if (exists)
            throw new InvalidOperationException($"Client ID '{clientId}' already exists.");

        // Generate secure secret
        var plaintextSecret = SecretHasher.GenerateSecret(32); // 32 bytes = 256 bits
        var secretHash = SecretHasher.HashSecret(plaintextSecret);

        // Create client
        var client = ApiClient.Create(
            clientId,
            secretHash,
            name,
            description,
            scopes,
            createdBy);

        _context.ApiClients.Add(client);
        await _context.SaveChangesAsync(cancellationToken);

        return (client, plaintextSecret);
    }

    /// <summary>
    /// Authenticates an API client using Client Credentials.
    /// </summary>
    public async Task<ApiClient?> AuthenticateAsync(
        string clientId,
        string clientSecret,
        CancellationToken cancellationToken = default)
    {
        var client = await _context.ApiClients
            .FirstOrDefaultAsync(c => c.ClientId == clientId, cancellationToken);

        if (client == null)
            return null;

        if (!client.IsActive)
            return null;

        if (!SecretHasher.VerifySecret(clientSecret, client.ClientSecretHash))
            return null;

        return client;
    }

    /// <summary>
    /// Gets an API client by ID.
    /// </summary>
    public async Task<ApiClient?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.ApiClients
            .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    /// <summary>
    /// Gets an API client by client ID.
    /// </summary>
    public async Task<ApiClient?> GetByClientIdAsync(string clientId, CancellationToken cancellationToken = default)
    {
        return await _context.ApiClients
            .FirstOrDefaultAsync(c => c.ClientId == clientId, cancellationToken);
    }

    /// <summary>
    /// Gets all API clients.
    /// </summary>
    public async Task<List<ApiClient>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _context.ApiClients
            .OrderBy(c => c.Name)
            .ToListAsync(cancellationToken);
    }

    /// <summary>
    /// Updates API client display details.
    /// </summary>
    public async Task<ApiClient> UpdateDetailsAsync(
        Guid clientId,
        string name,
        string? description,
        CancellationToken cancellationToken = default)
    {
        var client = await _context.ApiClients
            .FirstOrDefaultAsync(c => c.Id == clientId, cancellationToken);

        if (client == null)
            throw new InvalidOperationException($"API client with ID '{clientId}' not found.");

        client.UpdateDetails(name, description);
        await _context.SaveChangesAsync(cancellationToken);
        // UpdatedBy and UpdatedAt set automatically by AuditableEntityInterceptor
        return client;
    }

    /// <summary>
    /// Updates API client scopes.
    /// </summary>
    public async Task<ApiClient> UpdateScopesAsync(
        Guid clientId,
        IEnumerable<string> scopes,
        CancellationToken cancellationToken = default)
    {
        var client = await _context.ApiClients
            .FirstOrDefaultAsync(c => c.Id == clientId, cancellationToken);

        if (client == null)
            throw new InvalidOperationException($"API client with ID '{clientId}' not found.");

        client.UpdateScopes(scopes);
        await _context.SaveChangesAsync(cancellationToken);
        // UpdatedBy and UpdatedAt set automatically by AuditableEntityInterceptor
        return client;
    }

    /// <summary>
    /// Regenerates the secret for an API client.
    /// Returns the new plaintext secret (only time it will be visible).
    /// </summary>
    public async Task<string> RegenerateSecretAsync(
        Guid clientId,
        CancellationToken cancellationToken = default)
    {
        var client = await _context.ApiClients
            .FirstOrDefaultAsync(c => c.Id == clientId, cancellationToken);

        if (client == null)
            throw new InvalidOperationException($"API client with ID '{clientId}' not found.");

        var plaintextSecret = SecretHasher.GenerateSecret(32);
        var secretHash = SecretHasher.HashSecret(plaintextSecret);

        client.UpdateSecret(secretHash);
        await _context.SaveChangesAsync(cancellationToken);
        // UpdatedBy and UpdatedAt set automatically by AuditableEntityInterceptor

        return plaintextSecret;
    }

    /// <summary>
    /// Activates an API client.
    /// </summary>
    public async Task<ApiClient> ActivateAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var client = await _context.ApiClients
            .FirstOrDefaultAsync(c => c.Id == clientId, cancellationToken);

        if (client == null)
            throw new InvalidOperationException($"API client with ID '{clientId}' not found.");

        client.Activate();
        await _context.SaveChangesAsync(cancellationToken);
        // UpdatedBy and UpdatedAt set automatically by AuditableEntityInterceptor
        return client;
    }

    /// <summary>
    /// Deactivates an API client.
    /// </summary>
    public async Task<ApiClient> DeactivateAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var client = await _context.ApiClients
            .FirstOrDefaultAsync(c => c.Id == clientId, cancellationToken);

        if (client == null)
            throw new InvalidOperationException($"API client with ID '{clientId}' not found.");

        client.Deactivate();
        await _context.SaveChangesAsync(cancellationToken);
        // UpdatedBy and UpdatedAt set automatically by AuditableEntityInterceptor
        return client;
    }

    /// <summary>
    /// Soft-deletes an API client.
    /// The client will be marked as deleted but data retained for audit purposes.
    /// </summary>
    public async Task DeleteAsync(Guid clientId, CancellationToken cancellationToken = default)
    {
        var client = await _context.ApiClients
            .FirstOrDefaultAsync(c => c.Id == clientId, cancellationToken);

        if (client == null)
            throw new InvalidOperationException($"API client with ID '{clientId}' not found.");

        client.DeleteClient(); // Soft delete via BaseAuditableEntity
        await _context.SaveChangesAsync(cancellationToken);
        // DeletedBy and DeletedAt set automatically by AuditableEntityInterceptor
    }
}
