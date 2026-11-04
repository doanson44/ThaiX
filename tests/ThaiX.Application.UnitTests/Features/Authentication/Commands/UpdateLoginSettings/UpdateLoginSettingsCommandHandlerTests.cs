using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Authentication.Commands.UpdateLoginSettings;

namespace ThaiX.Application.UnitTests.Features.Authentication.Commands.UpdateLoginSettings;

public sealed class UpdateLoginSettingsCommandHandlerTests
{
    private readonly Mock<IIdentityUserService> _identityUserServiceMock = new();
    private readonly UpdateLoginSettingsCommandHandler _sut;

    public UpdateLoginSettingsCommandHandlerTests()
    {
        _sut = new UpdateLoginSettingsCommandHandler(_identityUserServiceMock.Object);
    }

    [Fact]
    public async Task Handle_WithEnabledPreference_ShouldDelegateToCurrentIdentityUser()
    {
        // Arrange
        var command = new UpdateLoginSettingsCommand(Guid.NewGuid(), true);

        // Act
        await _sut.Handle(command, CancellationToken.None);

        // Assert
        _identityUserServiceMock.Verify(
            x => x.SetEmailOtpLoginEnabledAsync(
                command.UserId,
                true,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
