using ThaiX.Application.Features.Users.Commands.SetUserLockout;

namespace ThaiX.Application.UnitTests.Features.Users.Commands.SetUserLockout;

public sealed class SetUserLockoutCommandValidatorTests
{
    private readonly SetUserLockoutCommandValidator _sut = new();

    [Fact]
    public void Validate_WithValidLockCommand_ShouldNotHaveErrors()
    {
        var command = new SetUserLockoutCommand
        {
            UserId = Guid.NewGuid(),
            IsLocked = true,
            Reason = "Suspicious activity"
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidUnlockCommand_ShouldNotHaveErrors()
    {
        var command = new SetUserLockoutCommand
        {
            UserId = Guid.NewGuid(),
            IsLocked = false
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveError()
    {
        var command = new SetUserLockoutCommand
        {
            UserId = Guid.Empty,
            IsLocked = true,
            Reason = "Test"
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetUserLockoutCommand.UserId));
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Validate_WhenLocked_WithoutReason_ShouldHaveError(string? reason)
    {
        var command = new SetUserLockoutCommand
        {
            UserId = Guid.NewGuid(),
            IsLocked = true,
            Reason = reason
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetUserLockoutCommand.Reason));
    }

    [Fact]
    public void Validate_WhenUnlocked_WithoutReason_ShouldNotHaveError()
    {
        var command = new SetUserLockoutCommand
        {
            UserId = Guid.NewGuid(),
            IsLocked = false,
            Reason = null
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public void Validate_WithValidDuration_ShouldNotHaveErrors()
    {
        var command = new SetUserLockoutCommand
        {
            UserId = Guid.NewGuid(),
            IsLocked = true,
            Reason = "Suspicious activity",
            LockoutDurationMinutes = 60
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-100)]
    public void Validate_WithInvalidDuration_ShouldHaveError(int duration)
    {
        var command = new SetUserLockoutCommand
        {
            UserId = Guid.NewGuid(),
            IsLocked = true,
            Reason = "Test",
            LockoutDurationMinutes = duration
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == nameof(SetUserLockoutCommand.LockoutDurationMinutes));
    }

    [Fact]
    public void Validate_WithNullDuration_ShouldNotHaveErrors()
    {
        // null duration = permanent lockout
        var command = new SetUserLockoutCommand
        {
            UserId = Guid.NewGuid(),
            IsLocked = true,
            Reason = "Permanent ban",
            LockoutDurationMinutes = null
        };

        var result = _sut.Validate(command);

        result.IsValid.Should().BeTrue();
    }
}
