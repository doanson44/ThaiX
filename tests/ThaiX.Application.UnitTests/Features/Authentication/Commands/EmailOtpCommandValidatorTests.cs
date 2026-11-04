using ThaiX.Application.Features.Authentication.Commands.RequestEmailOtp;
using ThaiX.Application.Features.Authentication.Commands.UpdateLoginSettings;
using ThaiX.Application.Features.Authentication.Commands.VerifyEmailOtp;

namespace ThaiX.Application.UnitTests.Features.Authentication.Commands;

public sealed class EmailOtpCommandValidatorTests
{
    [Fact]
    public void RequestEmailOtp_WithValidEmail_ShouldNotHaveErrors()
    {
        // Arrange
        var validator = new RequestEmailOtpCommandValidator();
        var command = new RequestEmailOtpCommand("user@thaix.test");

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Theory]
    [InlineData("")]
    [InlineData("not-an-email")]
    [InlineData("missing-domain@")]
    public void RequestEmailOtp_WithInvalidEmail_ShouldHaveError(string email)
    {
        // Arrange
        var validator = new RequestEmailOtpCommandValidator();
        var command = new RequestEmailOtpCommand(email);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(RequestEmailOtpCommand.Email));
    }

    [Fact]
    public void VerifyEmailOtp_WithSixDigitCode_ShouldNotHaveErrors()
    {
        // Arrange
        var validator = new VerifyEmailOtpCommandValidator();
        var command = new VerifyEmailOtpCommand(Guid.NewGuid(), "123456", false);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("12")]
    [InlineData("1234567a")]
    [InlineData("1234567890")]
    public void VerifyEmailOtp_WithInvalidCode_ShouldHaveError(string code)
    {
        // Arrange
        var validator = new VerifyEmailOtpCommandValidator();
        var command = new VerifyEmailOtpCommand(Guid.NewGuid(), code, false);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(VerifyEmailOtpCommand.Code));
    }

    [Fact]
    public void UpdateLoginSettings_WithUserId_ShouldNotHaveErrors()
    {
        // Arrange
        var validator = new UpdateLoginSettingsCommandValidator();
        var command = new UpdateLoginSettingsCommand(Guid.NewGuid(), true);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void UpdateLoginSettings_WithEmptyUserId_ShouldHaveError()
    {
        // Arrange
        var validator = new UpdateLoginSettingsCommandValidator();
        var command = new UpdateLoginSettingsCommand(Guid.Empty, true);

        // Act
        var result = validator.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
