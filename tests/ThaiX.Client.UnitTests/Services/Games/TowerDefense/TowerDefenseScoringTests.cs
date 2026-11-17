using ThaiX.Client.Services.Games.TowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.TowerDefense;

public sealed class TowerDefenseScoringTests
{
    [Fact]
    public void EnemyScores_ShouldBeMultiplesOfTen_AndBossShouldAwardOneHundred()
    {
        // Arrange
        var enemies = TowerDefenseData.Enemies;

        // Act
        var scores = enemies.Select(enemy => enemy.Score).ToArray();
        var bossScore = TowerDefenseData.Enemy(EnemyKind.Boss).Score;

        // Assert
        scores.Should().OnlyContain(score => score > 0 && score % 10 == 0);
        bossScore.Should().Be(100);
    }

    [Fact]
    public void Advance_WhenFirstWaveCompletes_ShouldAwardScoreInMultiplesOfTen()
    {
        // Arrange
        var match = new TowerDefenseMatch(
            TowerDefenseData.Maps[0],
            TowerDefenseData.Difficulties[1],
            endless: false);
        match.StartWave();

        // Act
        var ticks = 0;
        while (match.State == MatchState.Running && ticks++ < 10_000)
            match.Advance();

        // Assert
        match.State.Should().Be(MatchState.Planning);
        match.Score.Should().Be(20);
        (match.FinalScore % 10).Should().Be(0);
    }

    [Theory]
    [InlineData(0, false)]
    [InlineData(19, false)]
    [InlineData(20, true)]
    [InlineData(40, true)]
    [InlineData(41, false)]
    public void IsEndlessRewardWave_ShouldMatchEveryTwentiethWave(int wave, bool expected)
    {
        // Arrange

        // Act
        var isRewardWave = TowerDefenseData.IsEndlessRewardWave(wave);

        // Assert
        isRewardWave.Should().Be(expected);
    }
}
