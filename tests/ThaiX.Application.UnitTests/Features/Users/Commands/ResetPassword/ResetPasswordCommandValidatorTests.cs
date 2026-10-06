using ThaiX.Application.Features.Users.Commands.ResetPassword;

namespace ThaiX.Application.UnitTests.Features.Users.Commands.ResetPassword;

public sealed class ResetPasswordCommandValidatorTests
{
    private readonly ResetPasswordCommandValidator _sut = new();

    [Fact]
    public void Validate_WithValidUserId_ShouldNotHaveErrors()
    {
        var command = new ResetPasswordCommand
        {
            UserId = Guid.NewGuid()
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveError()
    {
        var command = new ResetPasswordCommand
        {
            UserId = Guid.Empty
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.UserId));
    }

    [Fact]
    public void Validate_WithValidNewPassword_ShouldNotHaveErrors()
    {
        var command = new ResetPasswordCommand
        {
            UserId = Guid.NewGuid(),
            NewPassword = "P@ssw0rd123"
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("short")]
    [InlineData("nouppercase1@")]
    [InlineData("NOLOWERCASE1@")]
    [InlineData("NoDigitHere@")]
    [InlineData("NoSpecial1a")]
    public void Validate_WithWeakNewPassword_ShouldHaveError(string password)
    {
        var command = new ResetPasswordCommand
        {
            UserId = Guid.NewGuid(),
            NewPassword = password
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(ResetPasswordCommand.NewPassword));
    }

    [Fact]
    public void Validate_WithNullNewPassword_ShouldNotHaveErrors()
    {
        // Null password means system-generated
        var command = new ResetPasswordCommand
        {
            UserId = Guid.NewGuid(),
            NewPassword = null
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_DefaultRequirePasswordChange_ShouldBeTrue()
    {
        var command = new ResetPasswordCommand
        {
            UserId = Guid.NewGuid()
        };

        command.RequirePasswordChange.Should().BeTrue();
    }
}
