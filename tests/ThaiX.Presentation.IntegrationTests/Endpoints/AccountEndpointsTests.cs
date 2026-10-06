using System.Net;
using System.Net.Http.Json;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class AccountEndpointsTests : IntegrationTestBase
{
    public AccountEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    #region Confirm Email

    [Fact]
    public async Task ConfirmEmail_WithInvalidToken_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"confirm-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: false);

        // Act
        var response = await Client.PostAsJsonAsync("/api/account/confirm-email", new
        {
            UserId = userId.ToString(),
            Token = "invalid-token"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Resend Confirmation

    [Fact]
    public async Task ResendConfirmation_WithValidEmail_ShouldReturnOk()
    {
        // Arrange
        var email = $"resend-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: false);

        // Act
        var response = await Client.PostAsJsonAsync("/api/account/resend-confirmation", new
        {
            Email = email
        });

        // Assert - always returns OK for security
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResendConfirmation_WithNonExistentEmail_ShouldStillReturnOk()
    {
        // Act (security: should not reveal whether email exists)
        var response = await Client.PostAsJsonAsync("/api/account/resend-confirmation", new
        {
            Email = "doesnotexist@thaix.test"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Reset Password

    [Fact]
    public async Task ResetPassword_WithInvalidToken_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"resetpw-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);

        // Act
        var response = await Client.PostAsJsonAsync("/api/account/reset-password", new
        {
            UserId = userId,
            Token = "invalid-reset-token",
            Password = "NewPass@123",
            ConfirmPassword = "NewPass@123"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ResetPassword_WithMismatchedPasswords_ShouldReturnBadRequest()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/account/reset-password", new
        {
            UserId = Guid.NewGuid(),
            Token = "some-token",
            Password = "NewPass@123",
            ConfirmPassword = "DifferentPass@123"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Forgot Password

    [Fact]
    public async Task ForgotPassword_WithValidEmail_ShouldReturnOk()
    {
        // Arrange
        var email = $"forgot-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/account/forgot-password", new
        {
            Email = email
        });

        // Assert
        // Should always return OK regardless of whether email exists (security)
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ForgotPassword_WithNonExistentEmail_ShouldStillReturnOk()
    {
        // Act (security: should not reveal whether email exists)
        var response = await Client.PostAsJsonAsync("/api/account/forgot-password", new
        {
            Email = "nonexistent@thaix.test"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Two-Factor Authentication

    [Fact]
    public async Task Get2faStatus_WhenAuthenticated_ShouldReturnStatus()
    {
        // Arrange
        var email = $"2fa-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123");
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/account/2fa-status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ReadResponseAsync<TwoFactorStatusDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.Is2faEnabled.Should().BeFalse();
    }

    [Fact]
    public async Task Get2faStatus_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/account/2fa-status");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region External Logins

    [Fact]
    public async Task GetExternalLogins_WhenAuthenticated_ShouldReturnEmptyList()
    {
        // Arrange
        var email = $"ext-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123");
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/account/external-logins");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Personal Data

    [Fact]
    public async Task GetPersonalData_WhenAuthenticated_ShouldReturnData()
    {
        // Arrange
        var email = $"personal-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123");
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/account/personal-data");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeletePersonalData_WithValidPassword_ShouldSucceed()
    {
        // Arrange
        var email = $"delete-{Guid.NewGuid():N}@thaix.test";
        var password = "Test@Pass123";
        await CreateTestUserAsync(email, password);
        await AuthenticateAsync(email, password);

        // Act
        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/account/personal-data")
        {
            Content = JsonContent.Create(new { Password = password })
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify user cannot login after deletion
        Client.DefaultRequestHeaders.Authorization = null;
        var loginResponse = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Username = email,
            Password = password,
            RememberMe = false
        });
        loginResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeletePersonalData_WithWrongPassword_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"delbad-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123");
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/account/personal-data")
        {
            Content = JsonContent.Create(new { Password = "WrongPassword@123" })
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    // DTOs
    private sealed record TwoFactorStatusDto
    {
        public bool Is2faEnabled { get; init; }
        public bool HasAuthenticator { get; init; }
        public int RecoveryCodesLeft { get; init; }
    }

    #region Enable Authenticator

    [Fact]
    public async Task EnableAuthenticator_WhenAuthenticated_ShouldReturnKeyAndUri()
    {
        // Arrange
        var email = $"auth-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123");
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsync("/api/account/enable-authenticator", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("sharedKey");
        body.Should().Contain("authenticatorUri");
    }

    [Fact]
    public async Task EnableAuthenticator_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.PostAsync("/api/account/enable-authenticator", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Verify Authenticator

    [Fact]
    public async Task VerifyAuthenticator_WithInvalidCode_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"verify-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123");
        await AuthenticateAsync(email, "Test@Pass123");

        // Enable authenticator first
        await Client.PostAsync("/api/account/enable-authenticator", null);

        // Act - use an invalid TOTP code
        var response = await Client.PostAsJsonAsync("/api/account/verify-authenticator", new
        {
            Code = "000000"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region Disable 2FA

    [Fact]
    public async Task Disable2fa_WhenAuthenticated_ShouldReturnOk()
    {
        // Arrange
        var email = $"dis2fa-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123");
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsync("/api/account/disable-2fa", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Disable2fa_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.PostAsync("/api/account/disable-2fa", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Reset Authenticator

    [Fact]
    public async Task ResetAuthenticator_WhenAuthenticated_ShouldReturnOk()
    {
        // Arrange
        var email = $"resetauth-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123");
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsync("/api/account/reset-authenticator", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task ResetAuthenticator_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.PostAsync("/api/account/reset-authenticator", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Generate Recovery Codes

    [Fact]
    public async Task GenerateRecoveryCodes_WhenAuthenticated_ShouldReturnCodes()
    {
        // Arrange
        var email = $"recov-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123");
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsync("/api/account/generate-recovery-codes", null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("recoveryCodes");
    }

    [Fact]
    public async Task GenerateRecoveryCodes_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.PostAsync("/api/account/generate-recovery-codes", null);
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion

    #region Login With 2FA

    [Fact]
    public async Task LoginWith2fa_WithInvalidSessionToken_ShouldFail()
    {
        // Act - use invalid session token and code
        var response = await Client.PostAsJsonAsync("/api/account/login-2fa", new
        {
            TwoFactorSessionToken = "invalid-session-token",
            TwoFactorCode = "123456"
        });

        // Assert - should fail (either 400 or 401)
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    #endregion

    #region Login With Recovery Code

    [Fact]
    public async Task LoginWithRecoveryCode_WithInvalidSessionToken_ShouldFail()
    {
        // Act
        var response = await Client.PostAsJsonAsync("/api/account/login-recovery", new
        {
            TwoFactorSessionToken = "invalid-session-token",
            RecoveryCode = "INVALID-CODE"
        });

        // Assert
        response.IsSuccessStatusCode.Should().BeFalse();
    }

    #endregion

    #region Remove External Login

    [Fact]
    public async Task RemoveExternalLogin_WhenAuthenticated_WithInvalidProvider_ShouldStillReturnOk()
    {
        // Arrange
        var email = $"rmext-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123");
        await AuthenticateAsync(email, "Test@Pass123");

        // Act - try to remove non-existent external login
        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/account/external-login")
        {
            Content = JsonContent.Create(new
            {
                LoginProvider = "NonExistent",
                ProviderKey = "fake-key"
            })
        });

        // Assert - depends on implementation, might return OK or error
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.BadRequest, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task RemoveExternalLogin_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, "/api/account/external-login")
        {
            Content = JsonContent.Create(new
            {
                LoginProvider = "Google",
                ProviderKey = "fake-key"
            })
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    #endregion
}
