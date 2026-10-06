using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Users.Commands.CreateUser;

namespace ThaiX.Application.UnitTests.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandHandlerTests
{
    private readonly Mock<IIdentityUserService> _identityUserServiceMock = new();
    private readonly Mock<ILogger<CreateUserCommandHandler>> _loggerMock = new();
    private readonly CreateUserCommandHandler _sut;

    public CreateUserCommandHandlerTests()
    {
        _sut = new CreateUserCommandHandler(
            _identityUserServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidCommand_ShouldReturnNewUserId()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var command = new CreateUserCommand
        {
            Email = "newuser@thaix.com",
            Password = "P@ssw0rd123",
            PhoneNumber = "+841234567890",
            Permissions = new[] { "Users.View" },
            RequirePasswordChange = false
        };

        _identityUserServiceMock
            .Setup(x => x.CreateUserAsync(
                command.Email,
                command.Password,
                command.PhoneNumber,
                command.Permissions,
                command.RequirePasswordChange,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedUserId);

        _identityUserServiceMock.Verify(
            x => x.CreateUserAsync(
                command.Email,
                command.Password,
                command.PhoneNumber,
                command.Permissions,
                command.RequirePasswordChange,
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithMinimalInput_ShouldPassNullOptionalFields()
    {
        // Arrange
        var expectedUserId = Guid.NewGuid();
        var command = new CreateUserCommand
        {
            Email = "minimal@thaix.com",
            Password = "P@ssw0rd123"
        };

        _identityUserServiceMock
            .Setup(x => x.CreateUserAsync(
                command.Email,
                command.Password,
                null,
                null,
                false,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedUserId);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(expectedUserId);
    }

    [Fact]
    public async Task Handle_WhenServiceThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "existing@thaix.com",
            Password = "P@ssw0rd123"
        };

        _identityUserServiceMock
            .Setup(x => x.CreateUserAsync(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<string?>(),
                It.IsAny<IEnumerable<string>?>(),
                It.IsAny<bool>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User already exists"));

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User already exists");
    }
}
