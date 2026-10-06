using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Users.Commands.UpdateUser;

namespace ThaiX.Application.UnitTests.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandHandlerTests
{
    private readonly Mock<IIdentityUserService> _identityUserServiceMock = new();
    private readonly Mock<ILogger<UpdateUserCommandHandler>> _loggerMock = new();
    private readonly UpdateUserCommandHandler _sut;

    public UpdateUserCommandHandlerTests()
    {
        _sut = new UpdateUserCommandHandler(
            _identityUserServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithAllFields_ShouldCallServiceWithCorrectParams()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            Email = "new@thaix.com",
            PhoneNumber = "+1234567890",
            EmailConfirmed = true,
            TwoFactorEnabled = false
        };

        _identityUserServiceMock
            .Setup(x => x.UpdateUserAsync(
                command.UserId,
                command.Email,
                command.PhoneNumber,
                command.EmailConfirmed,
                command.TwoFactorEnabled,
                It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().Be(MediatR.Unit.Value);
        _identityUserServiceMock.Verify(
            x => x.UpdateUserAsync(
                command.UserId,
                command.Email,
                command.PhoneNumber,
                true,
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Handle_WithEmailOnly_ShouldPassNullForOtherFields()
    {
        // Arrange
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            Email = "updated@thaix.com"
        };

        _identityUserServiceMock
            .Setup(x => x.UpdateUserAsync(
                command.UserId,
                "updated@thaix.com",
                null,
                null,
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
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            Email = "taken@thaix.com"
        };

        _identityUserServiceMock
            .Setup(x => x.UpdateUserAsync(
                It.IsAny<Guid>(),
                It.IsAny<string?>(),
                It.IsAny<string?>(),
                It.IsAny<bool?>(),
                It.IsAny<bool?>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Email is already taken"));

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Email is already taken");
    }
}
