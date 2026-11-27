using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineEnemyMotionTests
{
    [Fact]
    public void Advance_WhenGridPathTurns_EasesTheHullAngleInsteadOfSnapping()
    {
        var simulation = new FrontlineGameSimulation(
            FrontlinePath.FromGridPath(
            [
                new FrontlineGridPoint(2, 2),
                new FrontlineGridPoint(5, 2),
                new FrontlineGridPoint(5, 5)
            ]),
            0);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("Jeep1", 150, 1, 20, IsMetal: true));
        var enemy = simulation.Enemies.Single();

        for (var tick = 0; tick < 200 && enemy.MovementAngle == 0; tick++)
            simulation.Advance();

        var movementDegrees = enemy.MovementAngle * 180 / Math.PI;
        movementDegrees.Should().BeInRange(80, 90);
        // The legacy setAngle moves 30 percent toward the new heading on the turn frame.
        enemy.DesiredAngleDegrees.Should().BeGreaterThan(0);
        enemy.DesiredAngleDegrees.Should().BeApproximately(movementDegrees * 0.3, 0.001);
    }

    [Fact]
    public void Advance_WhenEnemyTurnsAgain_KeepsEasingFromThePreviousHullAngle()
    {
        var simulation = new FrontlineGameSimulation(
            FrontlinePath.FromGridPath(
            [
                new FrontlineGridPoint(2, 2),
                new FrontlineGridPoint(5, 2),
                new FrontlineGridPoint(5, 5),
                new FrontlineGridPoint(2, 5)
            ]),
            0);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("Tank1", 500, 1, 20, IsMetal: true));
        var enemy = simulation.Enemies.Single();

        var turns = 0;
        var desiredAfterFirstTurn = 0d;
        for (var tick = 0; tick < 600 && turns < 2; tick++)
        {
            var previous = enemy.MovementAngle;
            simulation.Advance();
            if (enemy.MovementAngle == previous)
                continue;

            turns++;
            if (turns == 1)
                desiredAfterFirstTurn = enemy.DesiredAngleDegrees;
        }

        turns.Should().Be(2);
        desiredAfterFirstTurn.Should().BeGreaterThan(0);

        var movementDegrees = enemy.MovementAngle * 180 / Math.PI;
        enemy.DesiredAngleDegrees.Should().BeGreaterThan(desiredAfterFirstTurn);
        enemy.DesiredAngleDegrees.Should().BeLessThan(movementDegrees);
    }
}
