using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;
using ThaiX.Domain.Common.Constants;
using ThaiX.Infrastructure.Identity;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class ApiClientEndpointsTests : IntegrationTestBase
{
    public ApiClientEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetApiClients_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.GetAsync("/api/api-clients");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateApiClient_WithWritePermission_ShouldReturnCreatedAndUsableSecret()
    {
        var email = $"api-client-create-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.ApiClientWrite });
        await AuthenticateAsync(email, "Test@Pass123");

        var clientId = $"partner-{Guid.NewGuid():N}";
        var response = await Client.PostAsJsonAsync("/api/api-clients", new
        {
            ClientId = clientId,
            Name = "Partner System",
            Description = "Created from integration test",
            Scopes = new[] { Permissions.MasterDataRead, Permissions.PriceAlertRead }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await ReadResponseAsync<CreateApiClientResponseDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.ClientId.Should().Be(clientId);
        envelope.Data.ClientSecret.Should().NotBeNullOrWhiteSpace();
        envelope.Data.Scopes.Should().BeEquivalentTo(new[] { Permissions.MasterDataRead, Permissions.PriceAlertRead });

        Client.DefaultRequestHeaders.Authorization = null;

        var tokenResponse = await Client.PostAsJsonAsync("/api/token/client-credentials", new
        {
            ClientId = clientId,
            ClientSecret = envelope.Data.ClientSecret,
            Scope = $"{Permissions.MasterDataRead} {Permissions.PriceAlertRead}"
        });

        tokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokenEnvelope = await tokenResponse.Content.ReadFromJsonAsync<TokenApiResponse>();
        tokenEnvelope!.Success.Should().BeTrue();
        tokenEnvelope.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
        tokenEnvelope.Data.Scope.Should().Contain(Permissions.MasterDataRead);
    }

    [Fact]
    public async Task GetApiClients_WithReadPermission_ShouldReturnSeededClients()
    {
        var seeded = await SeedApiClientAsync();

        var email = $"api-client-read-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.ApiClientRead });
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.GetAsync("/api/api-clients");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<IReadOnlyList<ApiClientResponseDto>>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.Should().Contain(client => client.Id == seeded.Id && client.ClientId == seeded.ClientId);
    }

    [Fact]
    public async Task UpdateScopes_WithoutManageScopesPermission_ShouldReturnForbidden()
    {
        var seeded = await SeedApiClientAsync();

        var email = $"api-client-noscope-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.ApiClientWrite });
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.PutAsJsonAsync($"/api/api-clients/{seeded.Id}/scopes", new
        {
            Scopes = new[] { Permissions.MasterDataRead }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task RegenerateSecret_WithWritePermission_ShouldInvalidateOldSecret()
    {
        var seeded = await SeedApiClientAsync(scopes: new[] { Permissions.MasterDataRead });

        var email = $"api-client-rotate-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.ApiClientWrite });
        await AuthenticateAsync(email, "Test@Pass123");

        var rotateResponse = await Client.PostAsync($"/api/api-clients/{seeded.Id}/regenerate-secret", null);

        rotateResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var rotateEnvelope = await ReadResponseAsync<RegenerateApiClientSecretResponseDto>(rotateResponse);
        rotateEnvelope.Success.Should().BeTrue();
        rotateEnvelope.Data.Should().NotBeNull();
        rotateEnvelope.Data!.ClientSecret.Should().NotBeNullOrWhiteSpace();
        rotateEnvelope.Data.ClientSecret.Should().NotBe(seeded.ClientSecret);

        Client.DefaultRequestHeaders.Authorization = null;

        var oldTokenResponse = await Client.PostAsJsonAsync("/api/token/client-credentials", new
        {
            seeded.ClientId,
            ClientSecret = seeded.ClientSecret,
            Scope = Permissions.MasterDataRead
        });

        oldTokenResponse.IsSuccessStatusCode.Should().BeFalse();

        var newTokenResponse = await Client.PostAsJsonAsync("/api/token/client-credentials", new
        {
            seeded.ClientId,
            ClientSecret = rotateEnvelope.Data.ClientSecret,
            Scope = Permissions.MasterDataRead
        });

        newTokenResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var tokenEnvelope = await newTokenResponse.Content.ReadFromJsonAsync<TokenApiResponse>();
        tokenEnvelope!.Success.Should().BeTrue();
        tokenEnvelope.Data!.AccessToken.Should().NotBeNullOrWhiteSpace();
    }

    private async Task<SeededApiClient> SeedApiClientAsync(IEnumerable<string>? scopes = null)
    {
        using var scope = Factory.Services.CreateScope();
        var apiClientService = scope.ServiceProvider.GetRequiredService<ApiClientService>();

        var clientId = $"seeded-{Guid.NewGuid():N}";
        var created = await apiClientService.CreateAsync(
            clientId,
            "Seeded Client",
            "Created by integration test",
            scopes ?? new[] { Permissions.MasterDataRead, Permissions.PriceAlertRead },
            Guid.NewGuid());

        return new SeededApiClient(created.Client.Id, clientId, created.PlaintextSecret);
    }

    private sealed record SeededApiClient(Guid Id, string ClientId, string ClientSecret);

    private record ApiClientResponseDto
    {
        public Guid Id { get; init; }
        public string ClientId { get; init; } = string.Empty;
        public string Name { get; init; } = string.Empty;
        public string? Description { get; init; }
        public bool IsActive { get; init; }
        public IReadOnlyCollection<string> Scopes { get; init; } = Array.Empty<string>();
    }

    private sealed record CreateApiClientResponseDto : ApiClientResponseDto
    {
        public string ClientSecret { get; init; } = string.Empty;
    }

    private sealed record RegenerateApiClientSecretResponseDto
    {
        public Guid Id { get; init; }
        public string ClientId { get; init; } = string.Empty;
        public string ClientSecret { get; init; } = string.Empty;
    }

    private sealed record TokenResponseDto
    {
        public string? AccessToken { get; init; }
        public string? TokenType { get; init; }
        public int ExpiresIn { get; init; }
        public string? Scope { get; init; }
    }

    private sealed record TokenApiResponse
    {
        public bool Success { get; init; }
        public TokenResponseDto? Data { get; init; }
        public ApiError? Error { get; init; }
    }
}
