using Microsoft.Extensions.Logging;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Users.Commands.ResetPassword;

namespace ThaiX.Application.UnitTests.Features.Users.Commands.ResetPassword;

public sealed class ResetPasswordCommandHandlerTests
{
    private readonly Mock<IIdentityUserService> _identityUserServiceMock = new();
    private readonly Mock<ILogger<ResetPasswordCommandHandler>> _loggerMock = new();
    private readonly ResetPasswordCommandHandler _sut;

    public ResetPasswordCommandHandlerTests()
    {
        _sut = new ResetPasswordCommandHandler(
            _identityUserServiceMock.Object,
            _loggerMock.Object);
    }

    [Fact]
    public async Task Handle_WithSystemGeneratedPassword_ShouldReturnResult()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            UserId = Guid.NewGuid(),
            NewPassword = null,
            RequirePasswordChange = true
        };

        var expectedResult = new ResetPasswordResult
        {
            TemporaryPassword = "TempPass@123",
            RequirePasswordChange = true
        };

        _identityUserServiceMock
            .Setup(x => x.ResetPasswordAsync(
                command.UserId,
                null,
                true,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.TemporaryPassword.Should().Be("TempPass@123");
        result.RequirePasswordChange.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_WithProvidedPassword_ShouldPassToService()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            UserId = Guid.NewGuid(),
            NewPassword = "NewP@ssw0rd",
            RequirePasswordChange = false
        };

        var expectedResult = new ResetPasswordResult
        {
            TemporaryPassword = null,
            RequirePasswordChange = false
        };

        _identityUserServiceMock
            .Setup(x => x.ResetPasswordAsync(
                command.UserId,
                "NewP@ssw0rd",
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedResult);

        // Act
        var result = await _sut.Handle(command, CancellationToken.None);

        // Assert
        result.TemporaryPassword.Should().BeNull();
        result.RequirePasswordChange.Should().BeFalse();
    }

    [Fact]
    public async Task Handle_WhenServiceThrows_ShouldPropagateException()
    {
        // Arrange
        var command = new ResetPasswordCommand
        {
            UserId = Guid.NewGuid()
        };

        _identityUserServiceMock
            .Setup(x => x.ResetPasswordAsync(
                It.IsAny<Guid>(),
                It.IsAny<string?>(),
                It.IsAny<bool>(),
                It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("User not found"));

        // Act
        var act = () => _sut.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("User not found");
    }
}
