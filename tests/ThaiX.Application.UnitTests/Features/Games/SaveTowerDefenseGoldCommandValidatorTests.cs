using ThaiX.Application.Features.Games.Commands.SaveTowerDefenseGold;

namespace ThaiX.Application.UnitTests.Features.Games;

public sealed class SaveTowerDefenseGoldCommandValidatorTests
{
    private readonly SaveTowerDefenseGoldCommandValidator _sut = new();

    [Fact]
    public void Validate_WithValidUserAndGold_ShouldNotHaveErrors()
    {
        // Arrange
        var command = new SaveTowerDefenseGoldCommand(Guid.NewGuid(), 1_250);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeTrue();
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(1_000_000_001)]
    public void Validate_WithGoldOutsideSupportedRange_ShouldHaveError(int gold)
    {
        // Arrange
        var command = new SaveTowerDefenseGoldCommand(Guid.NewGuid(), gold);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(SaveTowerDefenseGoldCommand.Gold));
    }

    [Fact]
    public void Validate_WithEmptyUserId_ShouldHaveError()
    {
        // Arrange
        var command = new SaveTowerDefenseGoldCommand(Guid.Empty, 100);

        // Act
        var result = _sut.Validate(command);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(error => error.PropertyName == nameof(SaveTowerDefenseGoldCommand.UserId));
    }
}
