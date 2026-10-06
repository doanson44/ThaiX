using ThaiX.Application.Features.Users.Commands.UpdateUser;

namespace ThaiX.Application.UnitTests.Features.Users.Commands.UpdateUser;

public sealed class UpdateUserCommandValidatorTests
{
    private readonly UpdateUserCommandValidator _sut = new();

    [Fact]
    public void Validate_WithValidEmail_ShouldNotHaveErrors()
    {
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            Email = "updated@thaix.com"
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("+1234567890")]
    [InlineData("0123456789")]
    public void Validate_WithValidPhoneNumber_ShouldNotHaveErrors(string phoneNumber)
    {
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            PhoneNumber = phoneNumber
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmailConfirmedOnly_ShouldNotHaveErrors()
    {
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            EmailConfirmed = true
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithTwoFactorEnabledOnly_ShouldNotHaveErrors()
    {
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            TwoFactorEnabled = false
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveError()
    {
        var command = new UpdateUserCommand
        {
            UserId = Guid.Empty,
            Email = "test@thaix.com"
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.UserId));
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    [InlineData("@missing.com")]
    public void Validate_WithInvalidEmail_ShouldHaveError(string email)
    {
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            Email = email
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.Email));
    }

    [Fact]
    public void Validate_WithEmailExceedingMaxLength_ShouldHaveError()
    {
        var longEmail = new string('a', 250) + "@test.com";
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            Email = longEmail
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12345678")]
    [InlineData("1234567890123456")]
    public void Validate_WithInvalidPhoneNumber_ShouldHaveError(string phoneNumber)
    {
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            PhoneNumber = phoneNumber
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(UpdateUserCommand.PhoneNumber));
    }

    [Fact]
    public void Validate_WithNoFieldsProvided_ShouldHaveError()
    {
        // At least one field must be provided for update
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid()
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithAllFieldsProvided_ShouldNotHaveErrors()
    {
        var command = new UpdateUserCommand
        {
            UserId = Guid.NewGuid(),
            Email = "new@thaix.com",
            PhoneNumber = "+1234567890",
            EmailConfirmed = true,
            TwoFactorEnabled = true
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
