using System.Net;
using System.Net.Http.Json;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class TokenEndpointsTests : IntegrationTestBase
{
    public TokenEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    #region POST /api/token/login

    [Fact]
    public async Task TokenLogin_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var email = $"tklogin-{Guid.NewGuid():N}@thaix.test";
        var password = "Test@Pass123";
        await CreateTestUserAsync(email, password, confirmEmail: true);

        // Act
        var response = await Client.PostAsJsonAsync("/api/token/login", new
        {
            Username = email,
            Password = password,
            RememberMe = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<TokenApiResponse>();
        body!.Success.Should().BeTrue();
        body.Data!.AccessToken.Should().NotBeNullOrEmpty();
        body.Data.TokenType.Should().Be("Bearer");
        body.Data.ExpiresIn.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task TokenLogin_WithInvalidCredentials_ShouldReturnUnauthorized()
    {
        var response = await Client.PostAsJsonAsync("/api/token/login", new
        {
            Username = "nonexistent@thaix.test",
            Password = "WrongPass@123",
            RememberMe = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task TokenLogin_WithEmptyFields_ShouldReturnBadRequest()
    {
        var response = await Client.PostAsJsonAsync("/api/token/login", new
        {
            Username = "",
            Password = "",
            RememberMe = false
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region POST /api/token/introspect

    [Fact]
    public async Task TokenIntrospect_WithValidToken_ShouldReturnActive()
    {
        // Arrange
        var email = $"intro-{Guid.NewGuid():N}@thaix.test";
        var password = "Test@Pass123";
        await CreateTestUserAsync(email, password, confirmEmail: true);
        var token = await AuthenticateAsync(email, password);

        // Act
        var response = await Client.PostAsJsonAsync("/api/token/introspect", new { Token = token });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IntrospectionApiResponse>();
        body!.Success.Should().BeTrue();
        body.Data!.Active.Should().BeTrue();
        body.Data.Email.Should().Be(email);
    }

    [Fact]
    public async Task TokenIntrospect_WithInvalidToken_ShouldReturnInactive()
    {
        // Arrange - authenticate first so we have auth header
        var email = $"intro2-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/token/introspect", new { Token = "invalid.jwt.token" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadFromJsonAsync<IntrospectionApiResponse>();
        body!.Success.Should().BeTrue();
        body.Data!.Active.Should().BeFalse();
    }

    [Fact]
    public async Task TokenIntrospect_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.PostAsJsonAsync("/api/token/introspect", new { Token = "some.token" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region POST /api/token/client-credentials

    [Fact]
    public async Task TokenClientCredentials_WithInvalidCredentials_ShouldFail()
    {
        // No ApiClient exists in test DB, so any credentials should fail
        var response = await Client.PostAsJsonAsync("/api/token/client-credentials", new
        {
            ClientId = "nonexistent-client",
            ClientSecret = "bad-secret",
            Scope = (string?)null
        });

        // Should return error (either 400 or 401 depending on implementation)
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    #endregion

    // DTOs for deserialization
    private sealed record TokenResponseDto
    {
        public string? AccessToken { get; init; }
        public string? TokenType { get; init; }
        public int ExpiresIn { get; init; }
        public string? Scope { get; init; }
    }

    private sealed record IntrospectionResponseDto
    {
        public bool Active { get; init; }
        public string? Sub { get; init; }
        public string? Email { get; init; }
        public string? ActorType { get; init; }
        public string? Scope { get; init; }
    }

    private sealed record TokenApiResponse
    {
        public bool Success { get; init; }
        public TokenResponseDto? Data { get; init; }
    }

    private sealed record IntrospectionApiResponse
    {
        public bool Success { get; init; }
        public IntrospectionResponseDto? Data { get; init; }
    }
}
