using ThaiX.Application.Features.Users.Commands.SetUserPermissions;

namespace ThaiX.Application.UnitTests.Features.Users.Commands.SetUserPermissions;

public sealed class SetUserPermissionsCommandValidatorTests
{
    private readonly SetUserPermissionsCommandValidator _sut = new();

    [Fact]
    public void Validate_WithValidInput_ShouldNotHaveErrors()
    {
        var command = new SetUserPermissionsCommand
        {
            UserId = Guid.NewGuid(),
            Permissions = new[] { "User.Read", "User.Write" }
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveError()
    {
        var command = new SetUserPermissionsCommand
        {
            UserId = Guid.Empty,
            Permissions = new[] { "User.Read" }
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetUserPermissionsCommand.UserId));
    }

    [Fact]
    public void Validate_WithEmptyPermissionsList_ShouldNotHaveErrors()
    {
        // Empty list is valid - removes all permissions
        var command = new SetUserPermissionsCommand
        {
            UserId = Guid.NewGuid(),
            Permissions = Array.Empty<string>()
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData("NoSeparator")]
    [InlineData(".NoFeature")]
    [InlineData("NoAction.")]
    public void Validate_WithInvalidPermissionFormat_ShouldHaveError(string permission)
    {
        var command = new SetUserPermissionsCommand
        {
            UserId = Guid.NewGuid(),
            Permissions = new[] { permission }
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithMixedValidAndInvalidPermissions_ShouldHaveError()
    {
        var command = new SetUserPermissionsCommand
        {
            UserId = Guid.NewGuid(),
            Permissions = new[] { "User.Read", "InvalidPerm", "User.Write" }
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public void Validate_WithMultipleValidPermissions_ShouldNotHaveErrors()
    {
        var command = new SetUserPermissionsCommand
        {
            UserId = Guid.NewGuid(),
            Permissions = new[] { "User.Read", "User.Write", "Role.Read", "System.Admin" }
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
