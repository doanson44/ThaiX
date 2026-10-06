using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Authentication.Commands.Login;

namespace ThaiX.Application.UnitTests.Features.Authentication.Commands.Login;

public sealed class LoginCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock = new();
    private readonly LoginCommandHandler _sut;

    public LoginCommandHandlerTests()
    {
        _sut = new LoginCommandHandler(_authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnSucceededResult()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "admin@thaix.com",
            Password = "P@ssw0rd123",
            RememberMe = false
        };

        var tokenResult = new TokenResult
        {
            AccessToken = "jwt-token-value",
            TokenType = "Bearer",
            ExpiresIn = 1800,
            Scope = "Users.View Users.Create"
        };

        _authServiceMock
            .Setup(x => x.AuthenticateUserAsync(
                command.Username, command.Password, false, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticationResult
            {
                Succeeded = true,
                RequiresTwoFactor = false,
                Token = tokenResult
            });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.RequiresTwoFactor.Should().BeFalse();
        result.Token.Should().NotBeNull();
        result.Token!.AccessToken.Should().Be("jwt-token-value");
        result.Token.TokenType.Should().Be("Bearer");
        result.Token.ExpiresIn.Should().Be(1800);

        _authServiceMock.Verify(
            x => x.AuthenticateUserAsync(command.Username, command.Password, false, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithRememberMe_ShouldCallAuthServiceWithRememberMeTrue()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "user@thaix.com",
            Password = "P@ssw0rd123",
            RememberMe = true
        };

        var tokenResult = new TokenResult
        {
            AccessToken = "jwt-token",
            TokenType = "Bearer",
            ExpiresIn = 43200 * 60,
            Scope = "Users.View"
        };

        _authServiceMock
            .Setup(x => x.AuthenticateUserAsync(
                command.Username, command.Password, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new AuthenticationResult
            {
                Succeeded = true,
                RequiresTwoFactor = false,
                Token = tokenResult
            });

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Succeeded.Should().BeTrue();
        result.Token.Should().NotBeNull();
        _authServiceMock.Verify(
            x => x.AuthenticateUserAsync(command.Username, command.Password, true, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithTwoFactorRequired_ShouldReturnTwoFactorResult()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "2fa-user@thaix.com",
            Password = "P@ssw0rd123",
            RememberMe = false
        };

        _authServiceMock
            .Setup(x => x.AuthenticateUserAsync(
                command.Username, command.Password, false, It.IsAny<CancellationToken>()))
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

    [Fact]
    public async Task Handle_WithInvalidCredentials_ShouldPropagateThrownException()
    {
        // Arrange
        var command = new LoginCommand
        {
            Username = "unknown@thaix.com",
            Password = "wrong-password",
            RememberMe = false
        };

        _authServiceMock
            .Setup(x => x.AuthenticateUserAsync(
                command.Username, command.Password, false, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("Invalid credentials"));

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<UnauthorizedAccessException>()
            .WithMessage("Invalid credentials");
    }
}
