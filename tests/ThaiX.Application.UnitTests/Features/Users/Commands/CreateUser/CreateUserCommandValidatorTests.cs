using ThaiX.Application.Features.Users.Commands.CreateUser;

namespace ThaiX.Application.UnitTests.Features.Users.Commands.CreateUser;

public sealed class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _sut = new();

    [Fact]
    public void Validate_WithValidInput_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "newuser@thaix.com",
            Password = "P@ssw0rd123"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("not-an-email")]
    [InlineData("missing@")]
    public void Validate_WithInvalidEmail_ShouldHaveError(string? email)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = email!,
            Password = "P@ssw0rd123"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Email));
    }

    [Fact]
    public void Validate_WithEmailExceedingMaxLength_ShouldHaveError()
    {
        // Arrange
        var longEmail = new string('a', 250) + "@test.com";
        var command = new CreateUserCommand
        {
            Email = longEmail,
            Password = "P@ssw0rd123"
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Email));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    [InlineData("short")]
    [InlineData("nouppercase1@")]
    [InlineData("NOLOWERCASE1@")]
    [InlineData("NoDigitHere@")]
    [InlineData("NoSpecial1a")]
    public void Validate_WithInvalidPassword_ShouldHaveError(string? password)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "user@thaix.com",
            Password = password!
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.Password));
    }

    [Theory]
    [InlineData("+1234567890")]
    [InlineData("+841234567890")]
    [InlineData("0123456789")]
    [InlineData(null)]
    public void Validate_WithValidPhoneNumber_ShouldNotHaveError(string? phoneNumber)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "user@thaix.com",
            Password = "P@ssw0rd123",
            PhoneNumber = phoneNumber
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12345678")]
    [InlineData("1234567890123456")]
    public void Validate_WithInvalidPhoneNumber_ShouldHaveError(string phoneNumber)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "user@thaix.com",
            Password = "P@ssw0rd123",
            PhoneNumber = phoneNumber
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(CreateUserCommand.PhoneNumber));
    }

    [Fact]
    public void Validate_WithValidPermissions_ShouldNotHaveError()
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "user@thaix.com",
            Password = "P@ssw0rd123",
            Permissions = new[] { "Users.View", "Users.Create" }
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("NoSeparator")]
    [InlineData(".NoFeature")]
    [InlineData("NoAction.")]
    public void Validate_WithInvalidPermission_ShouldHaveError(string permission)
    {
        // Arrange
        var command = new CreateUserCommand
        {
            Email = "user@thaix.com",
            Password = "P@ssw0rd123",
            Permissions = new[] { permission }
        };

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
    }
}
