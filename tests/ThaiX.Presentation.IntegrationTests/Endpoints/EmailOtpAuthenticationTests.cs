using System.Net;
using System.Net.Http.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ThaiX.Application.Common.Exceptions;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Infrastructure.Identity;
using ThaiX.Infrastructure.Persistence;
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
    public async Task RequestEmailOtp_WithinCooldown_ShouldReuseChallengeWithoutSendingAnotherEmail()
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
        secondChallenge.ChallengeId.Should().Be(firstChallenge.ChallengeId);
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

    private async Task EnableEmailOtpAsync(Guid userId, bool enableTwoFactor = false)
    {
        using var scope = Factory.Services.CreateScope();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.FindByIdAsync(userId.ToString());
        user.Should().NotBeNull();

        user!.EmailOtpLoginEnabled = true;
        user.TwoFactorEnabled = enableTwoFactor;
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
