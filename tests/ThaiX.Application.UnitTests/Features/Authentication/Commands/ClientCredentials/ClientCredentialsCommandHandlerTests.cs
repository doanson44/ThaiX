using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Authentication.Commands.ClientCredentials;

namespace ThaiX.Application.UnitTests.Features.Authentication.Commands.ClientCredentials;

public sealed class ClientCredentialsCommandHandlerTests
{
    private readonly Mock<IAuthenticationService> _authServiceMock = new();
    private readonly ClientCredentialsCommandHandler _sut;

    public ClientCredentialsCommandHandlerTests()
    {
        _sut = new ClientCredentialsCommandHandler(
            _authServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCredentials_ShouldReturnTokenDto()
    {
        // Arrange
        var command = new ClientCredentialsCommand
        {
            ClientId = "test-client",
            ClientSecret = "test-secret",
            Scope = "api.read"
        };

        var tokenResult = new TokenResult
        {
            AccessToken = "jwt-token-123",
            TokenType = "Bearer",
            ExpiresIn = 3600,
            Scope = "api.read"
        };

        _authServiceMock
            .Setup(x => x.AuthenticateClientAsync(
                command.ClientId,
                command.ClientSecret,
                command.Scope,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.AccessToken.Should().Be("jwt-token-123");
        result.TokenType.Should().Be("Bearer");
        result.ExpiresIn.Should().Be(3600);
    }

    [Fact]
    public async Task Handle_WithNullScope_ShouldPassNullToService()
    {
        // Arrange
        var command = new ClientCredentialsCommand
        {
            ClientId = "test-client",
            ClientSecret = "test-secret",
            Scope = null
        };

        var tokenResult = new TokenResult
        {
            AccessToken = "jwt-token",
            TokenType = "Bearer",
            ExpiresIn = 3600,
            Scope = ""
        };

        _authServiceMock
            .Setup(x => x.AuthenticateClientAsync(
                command.ClientId,
                command.ClientSecret,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(tokenResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        _authServiceMock.Verify(
            x => x.AuthenticateClientAsync("test-client", "test-secret", null, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WhenServiceThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new ClientCredentialsCommand
        {
            ClientId = "bad-client",
            ClientSecret = "bad-secret"
        };

        _authServiceMock
            .Setup(x => x.AuthenticateClientAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Invalid client"));

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Invalid client");
    }
}
