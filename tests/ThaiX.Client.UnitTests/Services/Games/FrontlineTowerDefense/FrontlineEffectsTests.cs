using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineEffectsTests
{
    private static readonly FrontlinePath Path = new(
    [
        new FrontlinePoint(80, 48),
        new FrontlinePoint(300, 48)
    ]);

    [Fact]
    public void Advance_WhenEnemyBurns_EmitsFlameParticleEveryFifthTick()
    {
        var simulation = new FrontlineGameSimulation(Path, 0);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("infantry", 500, 0.0001, 10));
        var enemy = simulation.Enemies.Single();
        simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(Burn: 300)).Should().BeTrue();

        for (var tick = 0; tick < 4; tick++)
            simulation.Advance();

        simulation.Particles.Should().NotContain(particle => particle.Strip == FrontlineSpriteStrip.Flame);

        simulation.Advance();

        simulation.Particles.Should().ContainSingle(particle =>
            particle.Strip == FrontlineSpriteStrip.Flame && particle.Glow);
    }

    [Fact]
    public void ApplyDamage_WhenInfantryDies_EmitsFourBloodParticlesAndBloodDecal()
    {
        var simulation = new FrontlineGameSimulation(Path, 0);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("enemy1", 50, 0.0001, 10));
        var enemy = simulation.Enemies.Single();

        simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(DirectDamage: 100)).Should().BeTrue();

        simulation.Particles.Should().HaveCount(4);
        simulation.Particles.Should().OnlyContain(particle =>
            particle.Strip == FrontlineSpriteStrip.Blood);
        simulation.Decals.Should().ContainSingle()
            .Which.Strip.Should().Be(FrontlineSpriteStrip.BloodDecal);
    }

    [Fact]
    public void ApplyDamage_WhenVehicleDies_EmitsFlameSmokeParticlesAndCrater()
    {
        var simulation = new FrontlineGameSimulation(Path, 0);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("Jeep1", 150, 0.0001, 20, IsMetal: true));
        var enemy = simulation.Enemies.Single();

        simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(DirectDamage: 500)).Should().BeTrue();

        simulation.Particles.Should().HaveCount(5);
        simulation.Particles.Count(particle => particle.Strip == FrontlineSpriteStrip.FlameShot)
            .Should().Be(2);
        simulation.Particles.Count(particle => particle.Strip == FrontlineSpriteStrip.Smoke)
            .Should().Be(3);
        simulation.Decals.Should().ContainSingle()
            .Which.Strip.Should().Be(FrontlineSpriteStrip.Crater);
    }

    [Fact]
    public void Advance_WhenRifleTowerFires_EmitsMuzzleFlashWeightedTracerAndBloodSplat()
    {
        var simulation = new FrontlineGameSimulation(Path, 50);
        var rifle = FrontlineTowerCatalog.Find("tower2")!;
        simulation.TryBuildTower(
            rifle.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50)).Should().BeTrue();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("infantry", 100, 0.0001, 10));

        simulation.Advance(1);
        simulation.Advance(0);

        simulation.Particles.Should().ContainSingle(particle =>
            particle.Strip == FrontlineSpriteStrip.MuzzleFlash && particle.Glow);
        simulation.Particles.Should().ContainSingle(particle =>
            particle.Strip == FrontlineSpriteStrip.Blood);
        simulation.CombatTraces.Should().ContainSingle().Which.Weight.Should().Be(2);
        simulation.Sparks.Should().BeEmpty();
    }

    [Fact]
    public void Advance_WhenRifleTowerHitsMetalTarget_EmitsSparkInsteadOfBlood()
    {
        var simulation = new FrontlineGameSimulation(Path, 50);
        var rifle = FrontlineTowerCatalog.Find("tower2")!;
        simulation.TryBuildTower(
            rifle.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50)).Should().BeTrue();
        simulation.SpawnEnemy(
            new FrontlineEnemyDefinition("vehicle", 500, 0.0001, 20, IsMetal: true));

        simulation.Advance(1);
        simulation.Advance(0);

        simulation.Sparks.Should().ContainSingle().Which.Life.Should().BeInRange(19, 20);
        simulation.Particles.Should().NotContain(particle =>
            particle.Strip == FrontlineSpriteStrip.Blood);
    }

    [Fact]
    public void Advance_WhenShotgunTowerFires_EmitsThreeWeightedPellets()
    {
        var simulation = new FrontlineGameSimulation(Path, 150);
        var shotgun = FrontlineTowerCatalog.Find("tower5")!;
        simulation.TryBuildTower(
            shotgun.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50)).Should().BeTrue();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("infantry", 500, 0.0001, 10));

        simulation.Advance(1);
        simulation.Advance(0);

        simulation.CombatTraces.Should().HaveCount(3);
        simulation.CombatTraces.Should().OnlyContain(trace =>
            trace.Kind == FrontlineCombatTraceKind.Shotgun && trace.Weight == 3);
    }

    [Fact]
    public void Advance_WhenMissileExplodes_EmitsFireExplosionSmokeAndCrater()
    {
        var simulation = new FrontlineGameSimulation(Path, 800);
        var missile = FrontlineTowerCatalog.Find("tower1")!;
        simulation.TryBuildTower(
            missile.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50)).Should().BeTrue();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("infantry", 500, 0.0001, 10));
        var enemy = simulation.Enemies.Single();

        simulation.Advance(1);
        simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(Burn: 50)).Should().BeTrue();
        simulation.Advance(1);
        simulation.Projectiles.Should().ContainSingle();

        for (var tick = 0; tick < 60 && simulation.Projectiles.Count > 0; tick++)
            simulation.Advance();

        simulation.Projectiles.Should().BeEmpty();
        simulation.Particles.Should().Contain(particle =>
            particle.Strip == FrontlineSpriteStrip.FireExplosion);
        simulation.Particles.Should().Contain(particle =>
            particle.Strip == FrontlineSpriteStrip.Explosion);
        simulation.Particles.Should().Contain(particle =>
            particle.Strip == FrontlineSpriteStrip.Smoke);
        simulation.Decals.Should().Contain(decal =>
            decal.Strip == FrontlineSpriteStrip.Crater);
    }

    [Fact]
    public void Advance_WhenSupportStrikeLands_EmitsExplosionAndFireCloudParticles()
    {
        var simulation = new FrontlineGameSimulation(Path, 0);
        var support = FrontlineTowerCatalog.Find("suptower1")!;
        simulation.TryBuildTower(
            support.CreateDefinition() with { RateOfFire = 100, ShotLimit = 1 },
            new FrontlinePoint(50, 50)).Should().BeTrue();

        simulation.Advance(1);
        simulation.SupportProjectiles.Should().ContainSingle();

        for (var tick = 0; tick < 200 && simulation.SupportProjectiles.Count > 0; tick++)
            simulation.Advance();

        simulation.SupportProjectiles.Should().BeEmpty();
        simulation.Particles.Should().Contain(particle =>
            particle.Strip == FrontlineSpriteStrip.Explosion);
        simulation.Decals.Should().Contain(decal =>
            decal.Strip == FrontlineSpriteStrip.Crater);
        simulation.AreaEffects.Should().ContainSingle();
    }

    [Fact]
    public void Advance_WhenEffectParticlesExpire_RemovesThemFromTheRenderList()
    {
        var simulation = new FrontlineGameSimulation(Path, 0);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("enemy1", 50, 0.0001, 10));
        var enemy = simulation.Enemies.Single();
        simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(DirectDamage: 100)).Should().BeTrue();
        simulation.Particles.Should().NotBeEmpty();

        for (var tick = 0; tick < 25; tick++)
            simulation.Advance();

        simulation.Particles.Should().BeEmpty();
        simulation.Decals.Should().NotBeEmpty();
    }
}
