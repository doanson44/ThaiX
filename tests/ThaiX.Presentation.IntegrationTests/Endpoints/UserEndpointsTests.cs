using System.Net;
using System.Net.Http.Json;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class UserEndpointsTests : IntegrationTestBase
{
    public UserEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    #region GET /api/users

    [Fact]
    public async Task GetPermissions_WithUserReadPermission_ShouldReturnOkAndListOfPermissions()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await Client.GetAsync("/api/users/permissions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<List<string>>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.Should().NotBeEmpty();
        envelope.Data.Should().Contain("User.Read");
        envelope.Data.Should().Contain("Contact.Read");
        envelope.Data.Should().Contain("MasterData.Read");
    }

    [Fact]
    public async Task GetPermissions_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.GetAsync("/api/users/permissions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPermissions_WithoutUserRead_ShouldReturnForbidden()
    {
        var email = $"noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.GetAsync("/api/users/permissions");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_WithAdminPermissions_ShouldReturnPagedUsers()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/users?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":true");
    }

    [Fact]
    public async Task GetUsers_WithSearchFilter_ShouldReturnFilteredResults()
    {
        // Arrange
        var email = $"searchme-{Guid.NewGuid():N}@thaix.test";
        await CreateAndAuthenticateAdminAsync(email);

        // Act
        var response = await Client.GetAsync($"/api/users?search=searchme");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetUsers_WithoutPermissions_ShouldReturnForbidden()
    {
        // Arrange - create user without permissions
        var email = $"noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUsers_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region GET /api/users/{userId}/permissions

    [Fact]
    public async Task GetUserPermissions_WithUserRead_ShouldReturnOkAndListOfPermissions()
    {
        await CreateAndAuthenticateAdminAsync();
        var targetEmail = $"getperms-{Guid.NewGuid():N}@thaix.test";
        var targetUserId = await CreateTestUserAsync(targetEmail, "Test@Pass123", confirmEmail: true);

        await Client.PutAsJsonAsync($"/api/users/{targetUserId}/permissions", new
        {
            Permissions = new[] { "User.Read", "Contact.Read" }
        });

        var response = await Client.GetAsync($"/api/users/{targetUserId}/permissions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<List<string>>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.Should().Contain("User.Read");
        envelope.Data.Should().Contain("Contact.Read");
        envelope.Data.Should().HaveCount(2);
    }

    [Fact]
    public async Task GetUserPermissions_WhenUserHasNoPermissions_ShouldReturnOkWithEmptyList()
    {
        await CreateAndAuthenticateAdminAsync();
        var targetEmail = $"noperms-{Guid.NewGuid():N}@thaix.test";
        var targetUserId = await CreateTestUserAsync(targetEmail, "Test@Pass123", confirmEmail: true);

        await Client.PutAsJsonAsync($"/api/users/{targetUserId}/permissions", new
        {
            Permissions = Array.Empty<string>()
        });

        var response = await Client.GetAsync($"/api/users/{targetUserId}/permissions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<List<string>>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.Should().BeEmpty();
    }

    [Fact]
    public async Task GetUserPermissions_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var userId = Guid.NewGuid();

        var response = await Client.GetAsync($"/api/users/{userId}/permissions");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetUserPermissions_WithoutUserRead_ShouldReturnForbidden()
    {
        var email = $"noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.GetAsync($"/api/users/{Guid.NewGuid()}/permissions");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetUserPermissions_WithNonExistentUserId_ShouldReturnOkWithEmptyList()
    {
        await CreateAndAuthenticateAdminAsync();
        var nonExistentUserId = Guid.NewGuid();

        var response = await Client.GetAsync($"/api/users/{nonExistentUserId}/permissions");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<List<string>>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.Should().BeEmpty();
    }

    #endregion

    #region POST /api/users

    [Fact]
    public async Task CreateUser_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var newUserEmail = $"newuser-{Guid.NewGuid():N}@thaix.test";

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", new
        {
            Email = newUserEmail,
            Password = "NewUser@Pass123",
            RequirePasswordChange = false,
            Permissions = Array.Empty<string>()
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":true");
    }

    [Fact]
    public async Task CreateUser_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", new
        {
            Email = "not-an-email",
            Password = "Test@Pass123",
            RequirePasswordChange = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateUser_WithoutPermissions_ShouldReturnForbidden()
    {
        // Arrange - user without UserWrite permission
        var email = $"noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/users", new
        {
            Email = "shouldfail@thaix.test",
            Password = "Test@Pass123",
            RequirePasswordChange = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region PUT /api/users/{userId}

    [Fact]
    public async Task UpdateUser_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var targetEmail = $"target-{Guid.NewGuid():N}@thaix.test";
        var targetUserId = await CreateTestUserAsync(targetEmail, "Test@Pass123", confirmEmail: true);

        var updateEmail = $"updated-{Guid.NewGuid():N}@thaix.test";

        // Act
        var response = await Client.PutAsJsonAsync($"/api/users/{targetUserId}", new
        {
            Email = updateEmail,
            EmailConfirmed = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UpdateUser_WithNonExistentId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var nonExistentId = Guid.NewGuid();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/users/{nonExistentId}", new
        {
            Email = "nope@thaix.test"
        });

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateUser_WithoutPermissions_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}", new
        {
            Email = "nope@thaix.test"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region PUT /api/users/{userId}/lockout

    [Fact]
    public async Task SetUserLockout_LockUser_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var targetEmail = $"lockme-{Guid.NewGuid():N}@thaix.test";
        var targetUserId = await CreateTestUserAsync(targetEmail, "Test@Pass123", confirmEmail: true);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/users/{targetUserId}/lockout", new
        {
            IsLocked = true,
            Reason = "Integration test lockout",
            LockoutDurationMinutes = 60
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SetUserLockout_UnlockUser_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var targetEmail = $"unlock-{Guid.NewGuid():N}@thaix.test";
        var targetUserId = await CreateTestUserAsync(targetEmail, "Test@Pass123", confirmEmail: true);

        // Lock first
        await Client.PutAsJsonAsync($"/api/users/{targetUserId}/lockout", new
        {
            IsLocked = true,
            Reason = "Test lock",
            LockoutDurationMinutes = 60
        });

        // Act - Unlock
        var response = await Client.PutAsJsonAsync($"/api/users/{targetUserId}/lockout", new
        {
            IsLocked = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SetUserLockout_WithoutPermissions_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}/lockout", new
        {
            IsLocked = true,
            Reason = "Test",
            LockoutDurationMinutes = 60
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region PUT /api/users/{userId}/permissions

    [Fact]
    public async Task SetUserPermissions_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var targetEmail = $"perms-{Guid.NewGuid():N}@thaix.test";
        var targetUserId = await CreateTestUserAsync(targetEmail, "Test@Pass123", confirmEmail: true);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/users/{targetUserId}/permissions", new
        {
            Permissions = new[] { "User.Read", "User.Write" }
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SetUserPermissions_ClearPermissions_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var targetEmail = $"noclear-{Guid.NewGuid():N}@thaix.test";
        var targetUserId = await CreateTestUserAsync(targetEmail, "Test@Pass123", confirmEmail: true);

        // Act - clear all permissions
        var response = await Client.PutAsJsonAsync($"/api/users/{targetUserId}/permissions", new
        {
            Permissions = Array.Empty<string>()
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SetUserPermissions_WithoutPermissions_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/users/{Guid.NewGuid()}/permissions", new
        {
            Permissions = new[] { "User.Read" }
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region POST /api/users/{userId}/reset-password

    [Fact]
    public async Task ResetPassword_WithAdminPermission_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var targetEmail = $"resetpw-{Guid.NewGuid():N}@thaix.test";
        var targetUserId = await CreateTestUserAsync(targetEmail, "Test@Pass123", confirmEmail: true);

        // Act
        var response = await Client.PostAsJsonAsync($"/api/users/{targetUserId}/reset-password", new
        {
            NewPassword = "Reset@Pass456",
            RequirePasswordChange = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetPassword_WithAutoGeneratedPassword_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var targetEmail = $"autogen-{Guid.NewGuid():N}@thaix.test";
        var targetUserId = await CreateTestUserAsync(targetEmail, "Test@Pass123", confirmEmail: true);

        // Act - no new password specified, system generates one
        var response = await Client.PostAsJsonAsync($"/api/users/{targetUserId}/reset-password", new
        {
            RequirePasswordChange = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("\"success\":true");
    }

    [Fact]
    public async Task ResetPassword_WithoutPermissions_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync($"/api/users/{Guid.NewGuid()}/reset-password", new
        {
            NewPassword = "Test@Pass456",
            RequirePasswordChange = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion

    #region User Profile And Contact Link

    [Fact]
    public async Task GetCurrentUserProfile_WhenAuthenticated_ShouldReturnOk()
    {
        // Arrange
        var email = $"profile-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/users/me/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetCurrentUserProfile_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/users/me/profile");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LinkUserToContact_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync($"/api/users/{Guid.NewGuid()}/link-contact", new
        {
            ContactId = Guid.NewGuid()
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task LinkUserToContact_WithoutUserReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"link-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync($"/api/users/{Guid.NewGuid()}/link-contact", new
        {
            ContactId = Guid.NewGuid()
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetLinkedContact_WhenAuthenticated_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var userId = await CreateTestUserAsync($"linked-{Guid.NewGuid():N}@thaix.test", "Test@Pass123", confirmEmail: true);

        // Act
        var response = await Client.GetAsync($"/api/users/{userId}/linked-contact");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UnlinkContact_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.DeleteAsync($"/api/users/{Guid.NewGuid()}/linked-contact");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UnlinkContact_WithoutUserWritePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"unlink-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { "User.Read" });
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.DeleteAsync($"/api/users/{Guid.NewGuid()}/linked-contact");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    #endregion
}
