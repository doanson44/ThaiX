namespace ThaiX.Client.Models.ApiClients;

public record ApiClientDto
{
    public Guid Id { get; init; }
    public string ClientId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public bool IsActive { get; init; }
    public IReadOnlyCollection<string> Scopes { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
}

public sealed record CreateApiClientRequest
{
    public string ClientId { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public IReadOnlyCollection<string>? Scopes { get; init; }
}

public sealed record UpdateApiClientRequest
{
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
}

public sealed record UpdateApiClientScopesRequest
{
    public IReadOnlyCollection<string>? Scopes { get; init; }
}

public sealed record CreateApiClientResponse : ApiClientDto
{
    public string ClientSecret { get; init; } = string.Empty;
}

public sealed record RegenerateApiClientSecretResponse
{
    public Guid Id { get; init; }
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
}
