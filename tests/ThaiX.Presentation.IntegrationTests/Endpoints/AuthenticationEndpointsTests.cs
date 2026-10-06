using System.Net;
using System.Net.Http.Json;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class AuthenticationEndpointsTests : IntegrationTestBase
{
    public AuthenticationEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    #region Register

    [Fact]
    public async Task Register_WithValidData_ShouldReturnOk()
    {
        // Arrange
        var request = new
        {
            Email = $"register-{Guid.NewGuid():N}@thaix.test",
            Password = "Test@Pass123",
            ConfirmPassword = "Test@Pass123"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ReadResponseAsync<RegisterResponseDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.Email.Should().Be(request.Email);
        envelope.Data.UserId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task Register_WithMismatchedPasswords_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            Email = "mismatch@thaix.test",
            Password = "Test@Pass123",
            ConfirmPassword = "Different@Pass123"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithWeakPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var request = new
        {
            Email = "weak-pass@thaix.test",
            Password = "123",
            ConfirmPassword = "123"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"dup-{Guid.NewGuid():N}@thaix.test";
        var request = new
        {
            Email = email,
            Password = "Test@Pass123",
            ConfirmPassword = "Test@Pass123"
        };

        // Register first time
        var firstResponse = await Client.PostAsJsonAsync("/api/auth/register", request);
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act - Register second time with same email
        var response = await Client.PostAsJsonAsync("/api/auth/register", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Login

    [Fact]
    public async Task Login_WithValidCredentials_ShouldReturnToken()
    {
        // Arrange
        var email = $"login-{Guid.NewGuid():N}@thaix.test";
        var password = "Test@Pass123";
        await CreateTestUserAsync(email, password, confirmEmail: true);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = email,
            Password = password,
            RememberMe = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ReadResponseAsync<LoginTokenResponse>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.Token.Should().NotBeNullOrEmpty();
        envelope.Data.TokenType.Should().Be("Bearer");
        envelope.Data.ExpiresIn.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Login_WithInvalidPassword_ShouldReturnUnauthorized()
    {
        // Arrange
        var email = $"badpass-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = email,
            Password = "WrongPass@123",
            RememberMe = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonExistentUser_ShouldReturnUnauthorized()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "nonexistent@thaix.test",
            Password = "Test@Pass123",
            RememberMe = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithEmptyFields_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = "",
            Password = "",
            RememberMe = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GetCurrentUser

    [Fact]
    public async Task GetCurrentUser_WhenAuthenticated_ShouldReturnUserInfo()
    {
        // Arrange
        var email = $"current-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123");
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/auth/user");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ReadResponseAsync<UserInfoDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.Email.Should().Be(email);
        envelope.Data.EmailConfirmed.Should().BeTrue();
    }

    [Fact]
    public async Task GetCurrentUser_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange - clear any existing auth header
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/auth/user");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region ChangePassword

    [Fact]
    public async Task ChangePassword_WithValidData_ShouldSucceed()
    {
        // Arrange
        var email = $"changepw-{Guid.NewGuid():N}@thaix.test";
        var oldPassword = "Test@Pass123";
        var newPassword = "NewTest@Pass456";
        await CreateTestUserAsync(email, oldPassword);
        await AuthenticateAsync(email, oldPassword);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/change-password", new
        {
            CurrentPassword = oldPassword,
            NewPassword = newPassword,
            ConfirmPassword = newPassword
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify can login with new password
        Client.DefaultRequestHeaders.Authorization = null;
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = email,
            Password = newPassword,
            RememberMe = false
        });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Login_WithRememberMe_ShouldReturnTokenWithExtendedExpiry()
    {
        // Arrange
        var email = $"remember-{Guid.NewGuid():N}@thaix.test";
        var password = "Test@Pass123";
        await CreateTestUserAsync(email, password, confirmEmail: true);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = email,
            Password = password,
            RememberMe = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<LoginTokenResponse>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.Token.Should().NotBeNullOrEmpty();
        envelope.Data.ExpiresIn.Should().BeGreaterThan(1800);
    }

    [Fact]
    public async Task ChangePassword_WithWrongCurrentPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"wrongcur-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123");
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/change-password", new
        {
            CurrentPassword = "WrongCurrent@123",
            NewPassword = "NewTest@Pass456",
            ConfirmPassword = "NewTest@Pass456"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Logout

    [Fact]
    public async Task Logout_WhenAuthenticated_ShouldReturnOk()
    {
        // Arrange
        var email = $"logout-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123");
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsync("/api/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Logout_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsync("/api/auth/logout", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    // DTOs for deserialization
    private sealed record RegisterResponseDto
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = default!;
        public bool RequiresEmailConfirmation { get; init; }
    }

    private sealed record UserInfoDto
    {
        public Guid UserId { get; init; }
        public string Email { get; init; } = default!;
        public bool EmailConfirmed { get; init; }
        public List<string> Permissions { get; init; } = [];
    }
}
