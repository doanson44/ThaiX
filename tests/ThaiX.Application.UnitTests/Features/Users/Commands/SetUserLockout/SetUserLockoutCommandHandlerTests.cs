using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Users.Commands.SetUserLockout;

namespace ThaiX.Application.UnitTests.Features.Users.Commands.SetUserLockout;

public sealed class SetUserLockoutCommandHandlerTests
{
    private readonly Mock<IIdentityUserService> _identityUserServiceMock = new();
    private readonly Mock<ILogger<SetUserLockoutCommandHandler>> _loggerMock = new();
    private readonly SetUserLockoutCommandHandler _sut;

    public SetUserLockoutCommandHandlerTests()
    {
        _sut = new SetUserLockoutCommandHandler(
            _identityUserServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_LockUser_ShouldCallServiceWithCorrectParams()
    {
        // Arrange
        var command = new SetUserLockoutCommand
        {
            UserId = Guid.NewGuid(),
            IsLocked = true,
            Reason = "Suspicious activity",
            LockoutDurationMinutes = 60
        };

        _identityUserServiceMock
            .Setup(x => x.SetUserLockoutAsync(
                command.UserId,
                true,
                60,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        _identityUserServiceMock.Verify(
            x => x.SetUserLockoutAsync(command.UserId, true, 60, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_UnlockUser_ShouldCallServiceWithFalse()
    {
        // Arrange
        var command = new SetUserLockoutCommand
        {
            UserId = Guid.NewGuid(),
            IsLocked = false
        };

        _identityUserServiceMock
            .Setup(x => x.SetUserLockoutAsync(
                command.UserId,
                false,
                null,
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
        var command = new SetUserLockoutCommand
        {
            UserId = Guid.NewGuid(),
            IsLocked = true,
            Reason = "Test"
        };

        _identityUserServiceMock
            .Setup(x => x.SetUserLockoutAsync(
                It.IsAny<Guid>(),
                It.IsAny<bool>(),
                It.IsAny<int?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User not found"));

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
