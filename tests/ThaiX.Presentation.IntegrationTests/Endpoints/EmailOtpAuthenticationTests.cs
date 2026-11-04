using System.Net;
using System.Net.Http.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Infrastructure.Identity;
using ThaiX.Infrastructure.Persistence;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class EmailOtpAuthenticationTests : IntegrationTestBase
{
    public EmailOtpAuthenticationTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task VerifyEmailOtp_WithValidCode_ShouldReturnJwtAndRejectReplay()
    {
        // Arrange
        var email = $"email-otp-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await EnableEmailOtpAsync(userId);
        var (challengeId, code) = await RequestChallengeAsync(email);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/email-otp/verify", new
        {
            ChallengeId = challengeId,
            Code = code,
            RememberMe = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<LoginTokenResponse>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.Token.Should().NotBeNullOrEmpty();
        envelope.Data.RequiresTwoFactor.Should().BeFalse();

        // A consumed challenge cannot be used a second time.
        var replay = await Client.PostAsJsonAsync("/api/auth/email-otp/verify", new
        {
            ChallengeId = challengeId,
            Code = code,
            RememberMe = false
        });
        replay.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RequestEmailOtp_ForEligibleUser_ShouldReturnChallengeMetadata()
    {
        // Arrange
        var email = $"email-otp-request-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await EnableEmailOtpAsync(userId);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/email-otp/request", new { Email = email });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<RequestEmailOtpDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.ChallengeId.Should().NotBeEmpty();
        envelope.Data.ExpiresAt.Should().BeAfter(DateTime.UtcNow);
        envelope.Data.ResendAfter.Should().BeAfter(DateTime.UtcNow);
        envelope.Data.CodeLength.Should().Be(6);
    }

    [Fact]
    public async Task RequestEmailOtp_ForUnconfirmedUser_ShouldReturnGenericResponseWithoutEmail()
    {
        // Arrange
        var email = $"email-otp-unconfirmed-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: false);
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId.ToString());
        user!.EmailOtpLoginEnabled = true;
        (await userManager.UpdateAsync(user)).Succeeded.Should().BeTrue();

        var otpService = scope.ServiceProvider.GetRequiredService<IEmailOtpService>();
        var emailSender = (FakeEmailSender)scope.ServiceProvider.GetRequiredService<IEmailSender>();

        // Act
        var result = await otpService.RequestAsync(email);

        // Assert
        result.ChallengeId.Should().NotBeEmpty();
        result.ExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);
        emailSender.SentEmails.Should().BeEmpty();
    }

    [Fact]
    public async Task RequestEmailOtp_WhenLoginPreferenceIsDisabled_ShouldNotSendEmail()
    {
        // Arrange
        var email = $"email-otp-disabled-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, confirmEmail: true);
        using var scope = Factory.Services.CreateScope();
        var otpService = scope.ServiceProvider.GetRequiredService<IEmailOtpService>();
        var emailSender = (FakeEmailSender)scope.ServiceProvider.GetRequiredService<IEmailSender>();

        // Act
        var result = await otpService.RequestAsync(email);

        // Assert
        result.ChallengeId.Should().NotBeEmpty();
        emailSender.SentEmails.Should().BeEmpty();
    }

    [Fact]
    public async Task RequestEmailOtp_ForUnknownEmail_ShouldReturnGenericResponse()
    {
        // Arrange
        var email = $"missing-{Guid.NewGuid():N}@thaix.test";

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/email-otp/request", new { Email = email });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<RequestEmailOtpDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.ChallengeId.Should().NotBeEmpty();
    }

    [Fact]
    public async Task VerifyEmailOtp_WithInvalidCode_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"email-otp-invalid-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await EnableEmailOtpAsync(userId);
        var (challengeId, code) = await RequestChallengeAsync(email);
        var incorrectCode = code == "999999" ? "000000" : "999999";

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/email-otp/verify", new
        {
            ChallengeId = challengeId,
            Code = incorrectCode,
            RememberMe = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task VerifyEmailOtp_WithRememberMe_ShouldUseExtendedJwtLifetime()
    {
        // Arrange
        var email = $"email-otp-remember-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await EnableEmailOtpAsync(userId);
        var (challengeId, code) = await RequestChallengeAsync(email);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/email-otp/verify", new
        {
            ChallengeId = challengeId,
            Code = code,
            RememberMe = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<LoginTokenResponse>(response);
        envelope.Data!.ExpiresIn.Should().BeGreaterThan(1800);
    }

    [Fact]
    public async Task VerifyEmailOtp_ShouldPreservePermissionScopesInJwt()
    {
        // Arrange
        var email = $"email-otp-permissions-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateUserWithPermissionsAsync(
            email,
            "Test@Pass123",
            [Permissions.UserRead],
            confirmEmail: true);
        await EnableEmailOtpAsync(userId);
        var (challengeId, code) = await RequestChallengeAsync(email);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/email-otp/verify", new
        {
            ChallengeId = challengeId,
            Code = code,
            RememberMe = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<LoginTokenResponse>(response);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(envelope.Data!.Token!);
        jwt.Claims
            .Where(claim => claim.Type == "scope")
            .Select(claim => claim.Value)
            .Should()
            .Contain(Permissions.UserRead);
    }

    [Fact]
    public async Task VerifyEmailOtp_WhenAccountIsLockedOut_ShouldReturnLockedStatus()
    {
        // Arrange
        var email = $"email-otp-locked-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await EnableEmailOtpAsync(userId);
        var (challengeId, code) = await RequestChallengeAsync(email);

        using (var scope = Factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(userId.ToString());
            await userManager.SetLockoutEnabledAsync(user!, true);
            await userManager.SetLockoutEndDateAsync(user!, DateTimeOffset.UtcNow.AddMinutes(10));
        }

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/email-otp/verify", new
        {
            ChallengeId = challengeId,
            Code = code,
            RememberMe = false
        });

        // Assert
        response.StatusCode.Should().Be((HttpStatusCode)423);
    }

    [Fact]
    public async Task VerifyEmailOtp_WhenAuthenticatorTwoFactorIsEnabled_ShouldRequireSecondFactor()
    {
        // Arrange
        var email = $"email-otp-2fa-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await EnableEmailOtpAsync(userId, enableTwoFactor: true);
        var (challengeId, code) = await RequestChallengeAsync(email);

        // Act
        var response = await Client.PostAsJsonAsync("/api/auth/email-otp/verify", new
        {
            ChallengeId = challengeId,
            Code = code,
            RememberMe = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<LoginTokenResponse>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.RequiresTwoFactor.Should().BeTrue();
        envelope.Data.TwoFactorSessionToken.Should().NotBeNullOrEmpty();
        envelope.Data.Token.Should().BeNull();
    }

    [Fact]
    public async Task VerifyEmailOtp_WithRememberMeAndAuthenticatorTwoFactor_ShouldUseExtendedJwtLifetime()
    {
        // Arrange
        var email = $"email-otp-2fa-remember-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await EnableEmailOtpAsync(userId, enableTwoFactor: true);
        var (challengeId, code) = await RequestChallengeAsync(email);

        var otpResponse = await Client.PostAsJsonAsync("/api/auth/email-otp/verify", new
        {
            ChallengeId = challengeId,
            Code = code,
            RememberMe = true
        });
        var otpEnvelope = await ReadResponseAsync<LoginTokenResponse>(otpResponse);
        otpEnvelope.Data!.RequiresTwoFactor.Should().BeTrue();

        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId.ToString());
        var authenticatorCode = await userManager.GenerateTwoFactorTokenAsync(
            user!,
            userManager.Options.Tokens.AuthenticatorTokenProvider);
        var identityUserService = scope.ServiceProvider.GetRequiredService<IIdentityUserService>();
        (await identityUserService.VerifyTwoFactorTokenAsync(userId, authenticatorCode)).Should().BeTrue();

        var sessionJwt = new JwtSecurityTokenHandler().ReadJwtToken(otpEnvelope.Data!.TwoFactorSessionToken!);
        sessionJwt.Claims.Single(x => x.Type == "remember_me").Value.Should().Be("True");

        // Act
        var response = await Client.PostAsJsonAsync("/api/account/login-2fa", new
        {
            TwoFactorSessionToken = otpEnvelope.Data.TwoFactorSessionToken,
            TwoFactorCode = authenticatorCode
        });

        // Assert
        response.StatusCode.Should().Be(
            HttpStatusCode.OK,
            await response.Content.ReadAsStringAsync());
        var envelope = await ReadResponseAsync<LoginTokenResponse>(response);
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(envelope.Data!.Token!);
        var issuedAt = long.Parse(jwt.Claims.Single(x => x.Type == JwtRegisteredClaimNames.Iat).Value);
        var issuedAtUtc = DateTimeOffset.FromUnixTimeSeconds(issuedAt).UtcDateTime;
        (jwt.ValidTo - issuedAtUtc).TotalMinutes.Should().BeApproximately(43200, 1);
    }

    [Fact]
    public async Task RequestEmailOtp_WithinCooldown_ShouldReturnGenericResponseWithoutSendingAnotherEmail()
    {
        // Arrange
        var email = $"email-otp-resend-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await EnableEmailOtpAsync(userId);

        using var scope = Factory.Services.CreateScope();
        var otpService = scope.ServiceProvider.GetRequiredService<IEmailOtpService>();
        var emailSender = (FakeEmailSender)scope.ServiceProvider.GetRequiredService<IEmailSender>();

        // Act
        var firstChallenge = await otpService.RequestAsync(email);
        var secondChallenge = await otpService.RequestAsync(email);

        // Assert
        secondChallenge.ChallengeId.Should().NotBe(firstChallenge.ChallengeId);
        secondChallenge.ExpiresAtUtc.Should().BeAfter(DateTime.UtcNow);
        emailSender.SentEmails.Should().ContainSingle();
    }

    [Fact]
    public async Task VerifyEmailOtp_ConcurrentRequests_ShouldConsumeChallengeOnlyOnce()
    {
        // Arrange
        var email = $"email-otp-concurrent-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await EnableEmailOtpAsync(userId);
        var (challengeId, code) = await RequestChallengeAsync(email);

        // Act
        var results = await Task.WhenAll(Enumerable.Range(0, 2).Select(async _ =>
        {
            using var scope = Factory.Services.CreateScope();
            var otpService = scope.ServiceProvider.GetRequiredService<IEmailOtpService>();
            try
            {
                return await otpService.VerifyAsync(challengeId, code);
            }
            catch (OperationFailedException)
            {
                return Guid.Empty;
            }
        }));

        // Assert
        results.Count(result => result == userId).Should().Be(1);
        results.Count(result => result == Guid.Empty).Should().Be(1);
    }

    [Fact]
    public async Task RequestEmailOtp_AfterCooldown_ShouldInvalidatePreviousChallenge()
    {
        // Arrange
        var email = $"email-otp-resend-old-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await EnableEmailOtpAsync(userId);
        var (oldChallengeId, oldCode) = await RequestChallengeAsync(email);

        using (var scope = Factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await dbContext.EmailOtpChallenges
                .Where(x => x.Id == oldChallengeId)
                .ExecuteUpdateAsync(setters => setters.SetProperty(
                    x => x.CreatedAtUtc,
                    DateTime.UtcNow.AddMinutes(-2)));
        }

        // Act
        using var resendScope = Factory.Services.CreateScope();
        var otpService = resendScope.ServiceProvider.GetRequiredService<IEmailOtpService>();
        var newChallenge = await otpService.RequestAsync(email);

        // Assert
        newChallenge.ChallengeId.Should().NotBe(oldChallengeId);
        var oldChallenge = await resendScope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>()
            .EmailOtpChallenges
            .AsNoTracking()
            .FirstAsync(x => x.Id == oldChallengeId);
        oldChallenge.InvalidatedAtUtc.Should().NotBeNull();
        await Assert.ThrowsAsync<OperationFailedException>(
            () => otpService.VerifyAsync(oldChallengeId, oldCode));
    }

    [Fact]
    public async Task RequestEmailOtp_ConcurrentRequests_ShouldCreateOnlyOneChallenge()
    {
        // Arrange
        var email = $"email-otp-concurrent-request-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await EnableEmailOtpAsync(userId);

        // Act
        var challengeResults = await Task.WhenAll(Enumerable.Range(0, 2).Select(async _ =>
        {
            using var scope = Factory.Services.CreateScope();
            var otpService = scope.ServiceProvider.GetRequiredService<IEmailOtpService>();
            return await otpService.RequestAsync(email);
        }));

        // Assert
        using var verifyScope = Factory.Services.CreateScope();
        var dbContext = verifyScope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var recentChallengeIds = await dbContext.EmailOtpChallenges
            .AsNoTracking()
            .Where(x => x.UserId == userId && x.CreatedAtUtc > DateTime.UtcNow.AddMinutes(-1))
            .Select(x => x.Id)
            .ToListAsync();
        recentChallengeIds.Should().ContainSingle();
        challengeResults.Count(x => x.ChallengeId == recentChallengeIds[0]).Should().Be(1);
    }

    [Fact]
    public async Task RequestEmailOtp_ShouldDeleteChallengesPastRetentionWindow()
    {
        // Arrange
        var email = $"email-otp-retention-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await EnableEmailOtpAsync(userId);
        var (oldChallengeId, _) = await RequestChallengeAsync(email);

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.EmailOtpChallenges
            .Where(x => x.Id == oldChallengeId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(
                x => x.ExpiresAtUtc,
                DateTime.UtcNow.AddDays(-2)));

        // Act
        var otpService = scope.ServiceProvider.GetRequiredService<IEmailOtpService>();
        var deletedCount = await otpService.CleanupExpiredAsync();
        var newChallenge = await otpService.RequestAsync(email);

        // Assert
        deletedCount.Should().BeGreaterThan(0);
        newChallenge.ChallengeId.Should().NotBe(oldChallengeId);
        (await dbContext.EmailOtpChallenges.AnyAsync(x => x.Id == oldChallengeId)).Should().BeFalse();
    }

    [Fact]
    public async Task VerifyEmailOtp_AfterExpiration_ShouldRejectCode()
    {
        // Arrange
        var email = $"email-otp-expired-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await EnableEmailOtpAsync(userId);
        var (challengeId, code) = await RequestChallengeAsync(email);

        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        await dbContext.EmailOtpChallenges
            .Where(x => x.Id == challengeId)
            .ExecuteUpdateAsync(setters => setters.SetProperty(
                x => x.ExpiresAtUtc,
                DateTime.UtcNow.AddMinutes(-1)));

        var otpService = scope.ServiceProvider.GetRequiredService<IEmailOtpService>();

        // Act
        var act = () => otpService.VerifyAsync(challengeId, code);

        // Assert
        await Assert.ThrowsAsync<OperationFailedException>(act);
    }

    [Fact]
    public async Task VerifyEmailOtp_AfterMaximumFailedAttempts_ShouldRejectCorrectCode()
    {
        // Arrange
        var email = $"email-otp-attempts-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await EnableEmailOtpAsync(userId);
        var (challengeId, code) = await RequestChallengeAsync(email);
        var incorrectCode = code == "999999" ? "000000" : "999999";

        using var scope = Factory.Services.CreateScope();
        var otpService = scope.ServiceProvider.GetRequiredService<IEmailOtpService>();

        // Act
        for (var attempt = 0; attempt < 5; attempt++)
        {
            await Assert.ThrowsAsync<OperationFailedException>(
                () => otpService.VerifyAsync(challengeId, incorrectCode));
        }

        var correctCodeAttempt = () => otpService.VerifyAsync(challengeId, code);

        // Assert
        await Assert.ThrowsAsync<OperationFailedException>(correctCodeAttempt);
    }

    [Fact]
    public async Task UpdateUser_WhenEmailChanges_ShouldDisableOtpAndInvalidateChallenges()
    {
        // Arrange
        var email = $"email-otp-email-change-{Guid.NewGuid():N}@thaix.test";
        var newEmail = $"email-otp-new-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await EnableEmailOtpAsync(userId);
        var (challengeId, _) = await RequestChallengeAsync(email);

        using var scope = Factory.Services.CreateScope();
        var identityUserService = scope.ServiceProvider.GetRequiredService<IIdentityUserService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

        // Act
        await identityUserService.UpdateUserAsync(
            userId,
            newEmail,
            phoneNumber: null,
            emailConfirmed: null,
            twoFactorEnabled: null);

        // Assert
        var user = await userManager.FindByIdAsync(userId.ToString());
        user!.EmailConfirmed.Should().BeFalse();
        user.EmailOtpLoginEnabled.Should().BeFalse();

        var challenge = await dbContext.EmailOtpChallenges
            .AsNoTracking()
            .FirstAsync(x => x.Id == challengeId);
        challenge.InvalidatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateLoginSettings_WhenAuthenticated_ShouldPersistEmailOtpPreference()
    {
        // Arrange
        var email = $"email-otp-settings-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await AuthenticateAsync(email);

        // Act
        var response = await Client.PutAsJsonAsync("/api/auth/login-settings", new
        {
            EmailOtpLoginEnabled = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId.ToString());
        user!.EmailOtpLoginEnabled.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateLoginSettings_WhenDisabled_ShouldInvalidateActiveChallenges()
    {
        // Arrange
        var email = $"email-otp-disable-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await EnableEmailOtpAsync(userId);
        var (challengeId, _) = await RequestChallengeAsync(email);
        await AuthenticateAsync(email);

        // Act
        var response = await Client.PutAsJsonAsync("/api/auth/login-settings", new
        {
            EmailOtpLoginEnabled = false
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        using var scope = Factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var challenge = await dbContext.EmailOtpChallenges
            .AsNoTracking()
            .FirstAsync(x => x.Id == challengeId);
        challenge.InvalidatedAtUtc.Should().NotBeNull();
    }

    [Fact]
    public async Task UpdateLoginSettings_WhenEmailIsUnconfirmed_ShouldReturnBadRequest()
    {
        // Arrange
        var email = $"email-otp-unconfirmed-setting-{Guid.NewGuid():N}@thaix.test";
        var userId = await CreateTestUserAsync(email, confirmEmail: true);
        await AuthenticateAsync(email);

        using (var scope = Factory.Services.CreateScope())
        {
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
            var user = await userManager.FindByIdAsync(userId.ToString());
            user!.EmailConfirmed = false;
            (await userManager.UpdateAsync(user)).Succeeded.Should().BeTrue();
        }

        // Act
        var response = await Client.PutAsJsonAsync("/api/auth/login-settings", new
        {
            EmailOtpLoginEnabled = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateLoginSettings_WhenAnonymous_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PutAsJsonAsync("/api/auth/login-settings", new
        {
            EmailOtpLoginEnabled = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    private sealed record RequestEmailOtpDto
    {
        public Guid ChallengeId { get; init; }
        public DateTime ExpiresAt { get; init; }
        public DateTime ResendAfter { get; init; }
        public int CodeLength { get; init; }
    }

    private async Task EnableEmailOtpAsync(Guid userId, bool enableTwoFactor = false)
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId.ToString());
        user.Should().NotBeNull();

        user!.EmailOtpLoginEnabled = true;
        user.TwoFactorEnabled = enableTwoFactor;
        if (enableTwoFactor)
        {
            var authenticatorKeyResult = await userManager.ResetAuthenticatorKeyAsync(user);
            authenticatorKeyResult.Succeeded.Should().BeTrue();
        }

        var result = await userManager.UpdateAsync(user);
        result.Succeeded.Should().BeTrue();
    }

    private async Task<(Guid ChallengeId, string Code)> RequestChallengeAsync(string email)
    {
        using var scope = Factory.Services.CreateScope();
        var otpService = scope.ServiceProvider.GetRequiredService<IEmailOtpService>();
        var emailSender = (FakeEmailSender)scope.ServiceProvider.GetRequiredService<IEmailSender>();

        var challenge = await otpService.RequestAsync(email);
        emailSender.SentEmails.Should().ContainSingle();
        emailSender.SentEmails.Single().Subject.Should().Be("ThaiX - Login Verification");

        var htmlBody = emailSender.SentEmails.Single().HtmlBody;
        var match = Regex.Match(htmlBody, @"<h1>(?<code>[0-9]{6})</h1>");
        match.Success.Should().BeTrue();

        return (challenge.ChallengeId, match.Groups["code"].Value);
    }
}
