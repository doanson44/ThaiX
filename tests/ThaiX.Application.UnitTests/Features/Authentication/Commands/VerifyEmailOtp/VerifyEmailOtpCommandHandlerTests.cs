using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Authentication.Commands.Login;
using ThaiX.Application.Features.Authentication.Commands.VerifyEmailOtp;

namespace ThaiX.Application.UnitTests.Features.Authentication.Commands.VerifyEmailOtp;

public sealed class VerifyEmailOtpCommandHandlerTests
{
    private readonly Mock<IEmailOtpService> _emailOtpServiceMock = new();
    private readonly Mock<IAuthenticationService> _authenticationServiceMock = new();
    private readonly VerifyEmailOtpCommandHandler _sut;

    public VerifyEmailOtpCommandHandlerTests()
    {
        _sut = new VerifyEmailOtpCommandHandler(
            _emailOtpServiceMock.Object,
            _authenticationServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCode_ShouldIssueJwtUsingRememberMePolicy()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new VerifyEmailOtpCommand(Guid.NewGuid(), "123456", true);
        _emailOtpServiceMock
            .Setup(x => x.VerifyAsync(command.ChallengeId, command.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _authenticationServiceMock
            .Setup(x => x.AuthenticateWithEmailOtpAsync(userId, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticationResult
            {
                Succeeded = true,
                Token = new TokenResult
                {
                    AccessToken = "jwt-token",
                    TokenType = "Bearer",
                    ExpiresIn = 43200,
                    Scope = "EmailOtp.Test.Read"
                }
            });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.RequiresTwoFactor.Should().BeFalse();
        result.Token!.AccessToken.Should().Be("jwt-token");
        result.Token.ExpiresIn.Should().Be(43200);
        _authenticationServiceMock.Verify(
            x => x.AuthenticateWithEmailOtpAsync(userId, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenAuthenticatorTwoFactorIsRequired_ShouldReturnTwoFactorChallenge()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var command = new VerifyEmailOtpCommand(Guid.NewGuid(), "123456", false);
        _emailOtpServiceMock
            .Setup(x => x.VerifyAsync(command.ChallengeId, command.Code, It.IsAny<CancellationToken>()))
            .ReturnsAsync(userId);

        _authenticationServiceMock
            .Setup(x => x.AuthenticateWithEmailOtpAsync(userId, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticationResult
            {
                Succeeded = false,
                RequiresTwoFactor = true,
                TwoFactorSessionToken = "2fa-session-token"
            });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeFalse();
        result.RequiresTwoFactor.Should().BeTrue();
        result.TwoFactorSessionToken.Should().Be("2fa-session-token");
        result.Token.Should().BeNull();
    }
}
