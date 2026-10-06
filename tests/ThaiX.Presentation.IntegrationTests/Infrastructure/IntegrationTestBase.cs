using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using ThaiX.Application.Common.Constants;
using ThaiX.Domain.Common.Constants;
using ThaiX.Infrastructure.Identity;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Infrastructure;

/// <summary>
/// Base class for integration tests. Provides access to the factory,
/// configured HttpClient, and helper methods for authentication.
/// </summary>
[Collection(IntegrationTestCollection.Name)]
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web)
    {
        Converters = { new JsonStringEnumConverter() }
    };

    protected ThaiXWebApplicationFactory Factory { get; }
    protected HttpClient Client { get; private set; } = null!;

    protected IntegrationTestBase(ThaiXWebApplicationFactory factory)
    {
        Factory = factory;
    }

    public virtual Task InitializeAsync()
    {
        Client = Factory.CreateClient();
        return Task.CompletedTask;
    }

    public virtual Task DisposeAsync()
    {
        Client?.Dispose();
        return Task.CompletedTask;
    }

    /// <summary>
    /// Creates a test user and returns the user ID.
    /// </summary>
    protected async Task<Guid> CreateTestUserAsync(
        string email = "testuser@thaix.com",
        string password = "Test@Pass123",
        bool confirmEmail = true)
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Remove existing user if present
        var existing = await userManager.FindByEmailAsync(email);
        if (existing is not null)
        {
            await userManager.DeleteAsync(existing);
        }

        var user = new ApplicationUser
        {
            UserName = email,
            Email = email,
            EmailConfirmed = confirmEmail
        };

        var result = await userManager.CreateAsync(user, password);
        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to create test user: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        return user.Id;
    }

    /// <summary>
    /// Authenticates with the given credentials and sets the Authorization header on the Client.
    /// Returns the login response.
    /// </summary>
    protected async Task<string> AuthenticateAsync(
        string email = "testuser@thaix.com",
        string password = "Test@Pass123")
    {
        var loginRequest = new { Username = email, Password = password, RememberMe = false };
        var response = await Client.PostAsJsonAsync("/api/auth/login", loginRequest);
        response.EnsureSuccessStatusCode();

        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<LoginTokenResponse>>();
        var token = envelope!.Data!.Token
            ?? throw new InvalidOperationException("Login did not return a token.");

        Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return token;
    }

    /// <summary>
    /// Creates a test user with specific permission claims and returns the user ID.
    /// </summary>
    protected async Task<Guid> CreateUserWithPermissionsAsync(
        string email,
        string password,
        IEnumerable<string> permissions,
        bool confirmEmail = true)
    {
        var userId = await CreateTestUserAsync(email, password, confirmEmail);

        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new InvalidOperationException("User not found after creation");

        var claims = permissions
            .Select(p => new Claim(ClaimTypeConstants.Permission, p))
            .ToList();

        if (claims.Count > 0)
        {
            var result = await userManager.AddClaimsAsync(user, claims);
            if (!result.Succeeded)
                throw new InvalidOperationException(
                    $"Failed to add claims: {string.Join(", ", result.Errors.Select(e => e.Description))}");
        }

        return userId;
    }

    /// <summary>
    /// Creates an admin user with all permissions and authenticates.
    /// Returns the user ID.
    /// </summary>
    protected async Task<Guid> CreateAndAuthenticateAdminAsync(
        string? email = null,
        string password = "Admin@Pass123")
    {
        email ??= $"admin-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateUserWithPermissionsAsync(
            email, password, Permissions.GetAll());
        await AuthenticateAsync(email, password);
        return userId;
    }

    /// <summary>
    /// Helper to read the standard API response envelope.
    /// </summary>
    protected static async Task<ApiResponse<T>> ReadResponseAsync<T>(HttpResponseMessage response)
    {
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        return envelope ?? throw new InvalidOperationException("Unable to deserialize API response.");
    }

    /// <summary>
    /// Helper to read the non-generic API response envelope.
    /// </summary>
    protected static async Task<ApiResponse> ReadResponseAsync(HttpResponseMessage response)
    {
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse>(JsonOptions);
        return envelope ?? throw new InvalidOperationException("Unable to deserialize API response.");
    }
}

/// <summary>
/// Internal DTO to read the login response token.
/// </summary>
internal sealed record LoginTokenResponse
{
    public string? Token { get; init; }
    public string? TokenType { get; init; }
    public int? ExpiresIn { get; init; }
    public bool RequiresTwoFactor { get; init; }
    public string? TwoFactorSessionToken { get; init; }
}
