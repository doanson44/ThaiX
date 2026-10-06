using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Users.Commands.SetUserPermissions;

namespace ThaiX.Application.UnitTests.Features.Users.Commands.SetUserPermissions;

public sealed class SetUserPermissionsCommandHandlerTests
{
    private readonly Mock<IIdentityUserService> _identityUserServiceMock = new();
    private readonly Mock<ILogger<SetUserPermissionsCommandHandler>> _loggerMock = new();
    private readonly SetUserPermissionsCommandHandler _sut;

    public SetUserPermissionsCommandHandlerTests()
    {
        _sut = new SetUserPermissionsCommandHandler(
            _identityUserServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithValidPermissions_ShouldCallService()
    {
        // Arrange
        var command = new SetUserPermissionsCommand
        {
            UserId = Guid.NewGuid(),
            Permissions = new[] { "User.Read", "User.Write" }
        };

        _identityUserServiceMock
            .Setup(x => x.SetUserPermissionsAsync(
                command.UserId,
                command.Permissions,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        _identityUserServiceMock.Verify(
            x => x.SetUserPermissionsAsync(command.UserId, command.Permissions, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmptyPermissions_ShouldCallService()
    {
        // Arrange
        var command = new SetUserPermissionsCommand
        {
            UserId = Guid.NewGuid(),
            Permissions = Array.Empty<string>()
        };

        _identityUserServiceMock
            .Setup(x => x.SetUserPermissionsAsync(
                command.UserId,
                command.Permissions,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
    }

    [Fact]
    public async Task Handle_WhenServiceThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new SetUserPermissionsCommand
        {
            UserId = Guid.NewGuid(),
            Permissions = new[] { "User.Read" }
        };

        _identityUserServiceMock
            .Setup(x => x.SetUserPermissionsAsync(
                It.IsAny<Guid>(),
                It.IsAny<IEnumerable<string>>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User not found"));

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
