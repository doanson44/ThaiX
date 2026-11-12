using ThaiX.Client.Services.Games;

namespace ThaiX.Client.UnitTests.Services.Games;

public sealed class TypingPassageCatalogTests
{
    [Theory]
    [InlineData(TypingDifficulty.Easy)]
    [InlineData(TypingDifficulty.Medium)]
    [InlineData(TypingDifficulty.Hard)]
    public void GetPassageKeys_ShouldReturnThreeUniquePassages(TypingDifficulty difficulty)
    {
        // Arrange

        // Act
        var keys = TypingPassageCatalog.GetPassageKeys(difficulty);

        // Assert
        keys.Should().HaveCount(3);
        keys.Should().OnlyHaveUniqueItems();
    }

    [Fact]
    public void SelectPassage_ShouldExcludeRecentlyUsedPassages()
    {
        // Arrange
        var passages = TypingPassageCatalog.GetPassageKeys(TypingDifficulty.Easy);
        var recent = passages.Take(2).ToArray();

        // Act
        var selected = TypingPassageCatalog.SelectPassage(
            TypingDifficulty.Easy,
            key => key,
            recent,
            new Random(0));

        // Assert
        selected.Should().Be(passages[2]);
    }

    [Fact]
    public void SelectPassage_ShouldReusePoolWhenEveryPassageWasRecentlyUsed()
    {
        // Arrange
        var passages = TypingPassageCatalog.GetPassageKeys(TypingDifficulty.Hard);

        // Act
        var selected = TypingPassageCatalog.SelectPassage(
            TypingDifficulty.Hard,
            key => key,
            passages,
            new Random(0));

        // Assert
        passages.Should().Contain(selected);
    }
}
