using ThaiX.Client.Services.Games.FrontlineTowerDefense;

namespace ThaiX.Client.UnitTests.Services.Games.FrontlineTowerDefense;

public sealed class FrontlineGameSimulationTests
{
    private static readonly FrontlinePath Path = new(
    [
        new FrontlinePoint(0, 0),
        new FrontlinePoint(100, 0)
    ]);

    [Fact]
    public void TryBuildTower_WithSufficientFunds_DeductsCostAndAddsTower()
    {
        var simulation = new FrontlineGameSimulation(Path, 500);
        var definition = new FrontlineTowerDefinition("rifle", 100, 50, 10, 100);

        var result = simulation.TryBuildTower(definition, new FrontlinePoint(25, 25));

        result.Should().BeTrue();
        simulation.Funds.Should().Be(400);
        simulation.Towers.Should().ContainSingle();
    }

    [Fact]
    public void TryBuildTower_WithInsufficientFunds_LeavesStateUnchanged()
    {
        var simulation = new FrontlineGameSimulation(Path, 50);
        var definition = new FrontlineTowerDefinition("rifle", 100, 50, 10, 5);

        var result = simulation.TryBuildTower(definition, new FrontlinePoint(25, 25));

        result.Should().BeFalse();
        simulation.Funds.Should().Be(50);
        simulation.Towers.Should().BeEmpty();
    }

    [Fact]
    public void UpgradeDamage_WithEnoughFunds_ChargesQuarterOfPurchaseCostAndAddsTwentyPercent()
    {
        var simulation = new FrontlineGameSimulation(Path, 500);
        simulation.TryBuildTower(
            new FrontlineTowerDefinition("rifle", 100, 50, 10, 5),
            new FrontlinePoint(25, 25));
        var tower = simulation.Towers.Single();
        var funds = 25;

        var upgraded = tower.UpgradeDamage(ref funds);

        upgraded.Should().BeTrue();
        funds.Should().Be(0);
        tower.Damage.Should().Be(12);
    }

    [Fact]
    public void Advance_WhenEnemyEntersTowerRange_ResolvesEnemyAndAwardsReward()
    {
        var simulation = new FrontlineGameSimulation(Path, 500);
        simulation.TryBuildTower(
            new FrontlineTowerDefinition("rifle", 100, 60, 25, 100),
            new FrontlinePoint(50, 10));
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("grunt", 20, 1, 15));

        simulation.Advance();
        simulation.Advance();

        simulation.Enemies.Should().BeEmpty();
        simulation.Funds.Should().Be(415);
        simulation.Score.Should().Be(30);
    }

    [Fact]
    public void Advance_WhenRifleTargetsInfantry_AppliesDirectAndBurnDamage()
    {
        var simulation = new FrontlineGameSimulation(Path, 500);
        var rifle = FrontlineTowerCatalog.Find("tower2")!;
        simulation.TryBuildTower(rifle.CreateDefinition(), new FrontlinePoint(50, 10));
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("infantry", 100, 0.1, 10));
        var enemy = simulation.Enemies.Single();

        simulation.Advance(34);
        simulation.Advance(34);

        enemy.Health.Should().Be(90);
        enemy.CoreTemperature.Should().Be(37);
    }

    [Fact]
    public void Advance_WhenRifleTargetsMetalEnemy_UsesReducedDirectDamageAndMetalBurn()
    {
        var simulation = new FrontlineGameSimulation(Path, 500);
        var rifle = FrontlineTowerCatalog.Find("tower2")!;
        simulation.TryBuildTower(rifle.CreateDefinition(), new FrontlinePoint(50, 10));
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("metal", 100, 0.1, 10, IsMetal: true));
        var enemy = simulation.Enemies.Single();

        simulation.Advance(34);
        simulation.Advance(34);

        enemy.Health.Should().Be(95);
        enemy.CoreTemperature.Should().Be(205);
    }

    [Fact]
    public void Advance_WhenNapalmSupportStrikeFires_AppliesAreaDamageAndExpiresAfterSixShots()
    {
        var simulation = new FrontlineGameSimulation(Path, 0, random: new Random(1));
        var support = FrontlineTowerCatalog.Find("suptower1")!;
        simulation.TryBuildTower(support.CreateDefinition(), new FrontlinePoint(50, 0));
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("grunt", 1000, 0.1, 10));
        var enemy = simulation.Enemies.Single();

        for (var index = 0; index < 6; index++)
            simulation.Advance(34);

        enemy.Health.Should().BeLessThan(1000);
        simulation.Towers.Should().BeEmpty();
    }

    [Fact]
    public void Advance_WhenElectroTowerIsUpgraded_AppliesReducedUpgradeToInfantry()
    {
        var path = new FrontlinePath(
        [
            new FrontlinePoint(80, 48),
            new FrontlinePoint(300, 48)
        ]);
        var simulation = new FrontlineGameSimulation(path, 500);
        var electro = FrontlineTowerCatalog.Find("tower4")!;
        simulation.TryBuildTower(electro.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50));
        var tower = simulation.Towers.Single();
        simulation.TryUpgradeTower(tower.Id, FrontlineTowerUpgradeType.Damage).Should().BeTrue();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("infantry", 500, 0.01, 10));
        var enemy = simulation.Enemies.Single();

        simulation.Advance(1);
        simulation.Advance(34);

        enemy.Health.Should().Be(444);
    }

    [Fact]
    public void Advance_WhenElectroTowerIsUpgraded_AppliesFullUpgradeToMetal()
    {
        var path = new FrontlinePath(
        [
            new FrontlinePoint(80, 48),
            new FrontlinePoint(300, 48)
        ]);
        var simulation = new FrontlineGameSimulation(path, 500);
        var electro = FrontlineTowerCatalog.Find("tower4")!;
        simulation.TryBuildTower(electro.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50));
        var tower = simulation.Towers.Single();
        simulation.TryUpgradeTower(tower.Id, FrontlineTowerUpgradeType.Damage).Should().BeTrue();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("vehicle", 500, 0.01, 20, IsMetal: true));
        var enemy = simulation.Enemies.Single();

        simulation.Advance(1);
        simulation.Advance(34);

        enemy.Health.Should().Be(440);
    }

    [Fact]
    public void Advance_WhenSupportTowerHasNoTarget_LaunchesStrike()
    {
        var path = new FrontlinePath(
        [
            new FrontlinePoint(80, 48),
            new FrontlinePoint(300, 48)
        ]);
        var simulation = new FrontlineGameSimulation(path, 0);
        var support = FrontlineTowerCatalog.Find("suptower1")!;
        simulation.TryBuildTower(support.CreateDefinition() with
        {
            RateOfFire = 100,
            ShotLimit = 1
        }, new FrontlinePoint(50, 50));

        simulation.Advance(1);

        simulation.SupportProjectiles.Should().ContainSingle();
        simulation.Towers.Should().BeEmpty();
        simulation.Status.Should().Be(FrontlineGameStatus.Planning);
    }

    [Fact]
    public void Advance_WhenNoEnemiesArePresent_ContinuesTowerReloading()
    {
        var path = new FrontlinePath(
        [
            new FrontlinePoint(80, 48),
            new FrontlinePoint(300, 48)
        ]);
        var simulation = new FrontlineGameSimulation(path, 200);
        var rifle = FrontlineTowerCatalog.Find("tower2")!;
        simulation.TryBuildTower(rifle.CreateDefinition() with { RateOfFire = 10 },
            new FrontlinePoint(50, 50));
        var tower = simulation.Towers.Single();

        simulation.Advance(5);

        tower.Loading.Should().Be(50);
        simulation.Status.Should().Be(FrontlineGameStatus.Planning);
    }

    [Fact]
    public void Advance_WhenWaveStartsAfterTowerReloads_FiresAfterAcquiringTarget()
    {
        var path = new FrontlinePath(
        [
            new FrontlinePoint(50, 48),
            new FrontlinePoint(300, 48)
        ]);
        var simulation = new FrontlineGameSimulation(path, 200);
        var rifle = FrontlineTowerCatalog.Find("tower2")!;
        simulation.TryBuildTower(rifle.CreateDefinition() with { RateOfFire = 10 },
            new FrontlinePoint(50, 50));
        var tower = simulation.Towers.Single();

        simulation.Advance(10);

        tower.Loading.Should().Be(100);
        simulation.Status.Should().Be(FrontlineGameStatus.Planning);

        simulation.SpawnEnemy(new FrontlineEnemyDefinition("infantry", 100, 0.0001, 10));
        var enemy = simulation.Enemies.Single();

        simulation.Advance(1);
        enemy.Health.Should().Be(100);
        tower.Loading.Should().Be(110);

        simulation.Advance(0);

        enemy.Health.Should().Be(100);
        tower.Loading.Should().Be(110);

        simulation.Advance(0.001);

        enemy.Health.Should().Be(90);
        tower.Loading.Should().Be(0);
    }

    [Fact]
    public void Advance_WhenMissileTargetIsNotHeated_DelaysLaunchUntilHeatThreshold()
    {
        var path = new FrontlinePath(
        [
            new FrontlinePoint(120, 48),
            new FrontlinePoint(300, 48)
        ]);
        var simulation = new FrontlineGameSimulation(path, 800);
        var missile = FrontlineTowerCatalog.Find("tower1")!;
        simulation.TryBuildTower(missile.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50));
        var tower = simulation.Towers.Single();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("infantry", 500, 0.01, 10));
        var enemy = simulation.Enemies.Single();

        simulation.Advance(1);
        simulation.Advance(34);

        simulation.Projectiles.Should().BeEmpty();
        tower.Loading.Should().Be(0);

        simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(Burn: 50)).Should().BeTrue();
        simulation.Advance(1);

        simulation.Projectiles.Should().ContainSingle();
    }

    [Fact]
    public void Advance_WhenMissileLauncherFires_TracksTargetAndAppliesSplashDamage()
    {
        var simulation = new FrontlineGameSimulation(Path, 800);
        var missile = FrontlineTowerCatalog.Find("tower1")!;
        simulation.TryBuildTower(
            missile.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(25, 25));
        var tower = simulation.Towers.Single();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("first", 500, 0.01, 10, IsMetal: true));
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("second", 500, 0.01, 10));
        var enemies = simulation.Enemies.ToArray();

        simulation.Advance(100);
        simulation.Advance(100);

        simulation.Projectiles.Should().ContainSingle();
        enemies.Should().OnlyContain(enemy => enemy.Health == 500);
        simulation.TrySellTower(tower.Id).Should().BeTrue();

        for (var tick = 0; tick < 10 && simulation.Projectiles.Count > 0; tick++)
            simulation.Advance();

        simulation.Projectiles.Should().BeEmpty();
        enemies.Should().OnlyContain(enemy => enemy.Health == 420);
        enemies[0].CoreTemperature.Should().Be(250);
    }

    [Theory]
    [InlineData("tower7", 9d, 500d)]
    [InlineData("tower8", 9d, 500d)]
    [InlineData("tower9", 10d, 50d)]
    [InlineData("tower11", 9d, 500d)]
    public void Find_WhenProjectileProfileIsCreated_MatchesLegacyRadiusAndLifetime(
        string towerId,
        double collisionRadius,
        double lifetime)
    {
        var profile = FrontlineTowerCatalog.Find(towerId)!.ProjectileProfile!;

        profile.CollisionRadius.Should().Be(collisionRadius);
        profile.Lifetime.Should().Be(lifetime);
    }

    [Theory]
    [InlineData("tower7")]
    [InlineData("tower8")]
    [InlineData("tower9")]
    [InlineData("tower11")]
    public void Find_WhenProjectileTowerHasNoLegacyDamageUpgrade_LeavesUpgradeScalesAtZero(string towerId)
    {
        var tower = FrontlineTowerCatalog.Find(towerId)!;

        tower.DamageUpgradeScale.Should().Be(0);
        tower.MetalDamageUpgradeScale.Should().Be(0);
        tower.StatusUpgradeScale.Should().Be(0);
    }

    [Fact]
    public void Advance_WhenBioCannonFires_UsesTrackingPoisonProjectile()
    {
        var path = new FrontlinePath(
        [
            new FrontlinePoint(80, 48),
            new FrontlinePoint(300, 48)
        ]);
        var simulation = new FrontlineGameSimulation(path, 300);
        var bioCannon = FrontlineTowerCatalog.Find("tower8")!;
        simulation.TryBuildTower(bioCannon.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50));
        var tower = simulation.Towers.Single();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("infantry", 100, 0.01, 10));
        var enemy = simulation.Enemies.Single();

        simulation.Advance(1);
        simulation.Advance(34);

        simulation.Projectiles.Should().ContainSingle();
        simulation.TrySellTower(tower.Id).Should().BeTrue();
        enemy.Health.Should().Be(100);

        for (var tick = 0; tick < 3; tick++)
            simulation.Advance();

        simulation.Projectiles.Should().BeEmpty();
        enemy.Health.Should().Be(88);
        enemy.PoisonDuration.Should().Be(60);
    }

    [Fact]
    public void Advance_WhenBioCannonIsDamageUpgraded_KeepsDamageAndPoisonUnchanged()
    {
        var path = new FrontlinePath(
        [
            new FrontlinePoint(80, 48),
            new FrontlinePoint(300, 48)
        ]);
        var simulation = new FrontlineGameSimulation(path, 600);
        var bioCannon = FrontlineTowerCatalog.Find("tower8")!;
        simulation.TryBuildTower(bioCannon.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50));
        var tower = simulation.Towers.Single();
        simulation.TryUpgradeTower(tower.Id, FrontlineTowerUpgradeType.Damage).Should().BeTrue();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("infantry", 500, 0.01, 10));
        var enemy = simulation.Enemies.Single();

        simulation.Advance(1);
        simulation.Advance(34);
        simulation.Projectiles.Should().ContainSingle();
        simulation.TrySellTower(tower.Id).Should().BeTrue();
        for (var tick = 0; tick < 10 && simulation.Projectiles.Count > 0; tick++)
            simulation.Advance();

        simulation.Projectiles.Should().BeEmpty();
        enemy.Health.Should().BeApproximately(488, 0.001);
        enemy.PoisonDuration.Should().BeApproximately(60, 0.001);
    }

    [Fact]
    public void Advance_WhenFlameThrowerFires_TracksTargetAndAppliesBurn()
    {
        var path = new FrontlinePath(
        [
            new FrontlinePoint(80, 48),
            new FrontlinePoint(300, 48)
        ]);
        var simulation = new FrontlineGameSimulation(path, 250);
        var flame = FrontlineTowerCatalog.Find("tower9")!;
        simulation.TryBuildTower(flame.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50));
        var tower = simulation.Towers.Single();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("infantry", 100, 0.01, 10));
        var enemy = simulation.Enemies.Single();

        simulation.Advance(1);
        simulation.Advance(34);

        simulation.Projectiles.Should().ContainSingle();
        simulation.TrySellTower(tower.Id).Should().BeTrue();
        for (var tick = 0; tick < 6 && simulation.Projectiles.Count > 0; tick++)
            simulation.Advance();

        simulation.Projectiles.Should().BeEmpty();
        enemy.Health.Should().Be(98);
        enemy.CoreTemperature.Should().Be(65);
    }

    [Fact]
    public void Advance_WhenFlameProjectileExceedsLegacyLifetime_ExpiresWithoutDamage()
    {
        var path = new FrontlinePath(
        [
            new FrontlinePoint(80, 48),
            new FrontlinePoint(300, 48)
        ]);
        var simulation = new FrontlineGameSimulation(path, 250);
        var flame = FrontlineTowerCatalog.Find("tower9")!;
        simulation.TryBuildTower(flame.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50));
        var tower = simulation.Towers.Single();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("fast", 500, 3, 10));
        var enemy = simulation.Enemies.Single();

        simulation.Advance(1);
        simulation.Advance(0.001);

        simulation.Projectiles.Should().ContainSingle();
        simulation.TrySellTower(tower.Id).Should().BeTrue();

        simulation.Advance(51);

        simulation.Projectiles.Should().BeEmpty();
        enemy.Health.Should().Be(500);
    }

    [Fact]
    public void Advance_WhenBallisticShellLaunches_UsesFortyPixelTargetLead()
    {
        var path = new FrontlinePath(
        [
            new FrontlinePoint(120, 48),
            new FrontlinePoint(300, 48)
        ]);
        var simulation = new FrontlineGameSimulation(path, 950);
        var mortar = FrontlineTowerCatalog.Find("tower7")!;
        simulation.TryBuildTower(mortar.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50));
        var tower = simulation.Towers.Single();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("vehicle", 500, 0.01, 10));

        simulation.Advance(1);
        simulation.Advance(0.001);
        var projectile = simulation.Projectiles.Single();
        simulation.TrySellTower(tower.Id).Should().BeTrue();

        simulation.Advance(1);

        projectile.Height.Should().BeApproximately(2.89, 0.02);
    }

    [Fact]
    public void Advance_WhenBallisticShellPassesItsApex_BeginsMovingTowardTarget()
    {
        var path = new FrontlinePath(
        [
            new FrontlinePoint(80, 48),
            new FrontlinePoint(300, 48)
        ]);
        var simulation = new FrontlineGameSimulation(path, 950);
        var mortar = FrontlineTowerCatalog.Find("tower7")!;
        var projectileProfile = mortar.ProjectileProfile! with { VerticalVelocityScale = 0.005 };
        simulation.TryBuildTower(mortar.CreateDefinition() with
        {
            RateOfFire = 100,
            ProjectileProfile = projectileProfile
        }, new FrontlinePoint(50, 50));
        var tower = simulation.Towers.Single();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("vehicle", 500, 0.01, 10));

        simulation.Advance(1);
        simulation.Advance(0.001);

        var projectile = simulation.Projectiles.Single();
        var launchPosition = projectile.Position;
        simulation.TrySellTower(tower.Id).Should().BeTrue();

        simulation.Advance(8);

        projectile.Position.X.Should().BeGreaterThan(launchPosition.X);
        projectile.Height.Should().BeApproximately(1.355, 0.01);
    }

    [Fact]
    public void Advance_WhenBallisticTargetDiesBeforeImpact_StillAppliesSplashDamage()
    {
        var path = new FrontlinePath(
        [
            new FrontlinePoint(120, 48),
            new FrontlinePoint(300, 48)
        ]);
        var nearbyPath = new FrontlinePath(
        [
            new FrontlinePoint(120, 100),
            new FrontlinePoint(300, 100)
        ]);
        var simulation = new FrontlineGameSimulation(path, 950);
        var mortar = FrontlineTowerCatalog.Find("tower7")!;
        simulation.TryBuildTower(mortar.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50));
        var tower = simulation.Towers.Single();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("target", 500, 0.01, 10));
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("nearby", 500, 0.01, 10), nearbyPath);
        var target = simulation.Enemies[0];
        var nearbyEnemy = simulation.Enemies[1];

        simulation.Advance(1);
        simulation.Advance(0.001);

        simulation.Projectiles.Should().ContainSingle();
        simulation.TrySellTower(tower.Id).Should().BeTrue();
        simulation.ApplyDamage(target.Id, new FrontlineDamageProfile(DirectDamage: 500))
            .Should().BeTrue();

        for (var tick = 0; tick < 70 && simulation.Projectiles.Count > 0; tick++)
            simulation.Advance();

        simulation.Projectiles.Should().BeEmpty();
        nearbyEnemy.Health.Should().Be(480);
        nearbyEnemy.PoisonDuration.Should().Be(30);
    }

    [Fact]
    public void Advance_WhenBioMortarShellImpacts_AppliesHalfDamageInArea()
    {
        var path = new FrontlinePath(
        [
            new FrontlinePoint(120, 48),
            new FrontlinePoint(300, 48)
        ]);
        var simulation = new FrontlineGameSimulation(path, 950);
        var mortar = FrontlineTowerCatalog.Find("tower7")!;
        simulation.TryBuildTower(mortar.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50));
        var tower = simulation.Towers.Single();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("vehicle", 500, 0.01, 10));
        var enemy = simulation.Enemies.Single();
        var nearbyPath = new FrontlinePath(
        [
            new FrontlinePoint(120, 100),
            new FrontlinePoint(300, 100)
        ]);
        var distantPath = new FrontlinePath(
        [
            new FrontlinePoint(120, 200),
            new FrontlinePoint(300, 200)
        ]);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("nearby", 500, 0.01, 10), nearbyPath);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("distant", 500, 0.01, 10), distantPath);
        var nearbyEnemy = simulation.Enemies[1];
        var distantEnemy = simulation.Enemies[2];

        simulation.Advance(1);
        simulation.Advance(34);

        simulation.Projectiles.Should().ContainSingle();
        simulation.Enemies.Should().OnlyContain(candidate => candidate.Health == 500);
        simulation.TrySellTower(tower.Id).Should().BeTrue();

        for (var tick = 0; tick < 70 && simulation.Projectiles.Count > 0; tick++)
            simulation.Advance();

        simulation.Projectiles.Should().BeEmpty();
        enemy.Health.Should().Be(480);
        enemy.PoisonDuration.Should().Be(30);
        nearbyEnemy.Health.Should().Be(480);
        nearbyEnemy.PoisonDuration.Should().Be(30);
        distantEnemy.Health.Should().Be(500);
        distantEnemy.PoisonDuration.Should().Be(0);
    }

    [Fact]
    public void Advance_WhenPlasmaCannonShellImpacts_AppliesRadiationSplashDamage()
    {
        var path = new FrontlinePath(
        [
            new FrontlinePoint(120, 48),
            new FrontlinePoint(300, 48)
        ]);
        var simulation = new FrontlineGameSimulation(path, 1000);
        var plasma = FrontlineTowerCatalog.Find("tower11")!;
        simulation.TryBuildTower(plasma.CreateDefinition() with { RateOfFire = 100 },
            new FrontlinePoint(50, 50));
        var tower = simulation.Towers.Single();
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("infantry", 500, 0.01, 10));
        var enemy = simulation.Enemies.Single();
        var nearbyPath = new FrontlinePath(
        [
            new FrontlinePoint(120, 100),
            new FrontlinePoint(300, 100)
        ]);
        var distantPath = new FrontlinePath(
        [
            new FrontlinePoint(120, 200),
            new FrontlinePoint(300, 200)
        ]);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("nearby", 500, 0.01, 10), nearbyPath);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("distant", 500, 0.01, 10), distantPath);
        var nearbyEnemy = simulation.Enemies[1];
        var distantEnemy = simulation.Enemies[2];

        simulation.Advance(1);
        simulation.Advance(34);

        simulation.Projectiles.Should().ContainSingle();
        simulation.Enemies.Should().OnlyContain(candidate => candidate.Health == 500);
        simulation.TrySellTower(tower.Id).Should().BeTrue();

        for (var tick = 0; tick < 70 && simulation.Projectiles.Count > 0; tick++)
            simulation.Advance();

        simulation.Projectiles.Should().BeEmpty();
        enemy.Health.Should().Be(480);
        enemy.RadiationDuration.Should().Be(20);
        nearbyEnemy.Health.Should().Be(480);
        nearbyEnemy.RadiationDuration.Should().Be(20);
        distantEnemy.Health.Should().Be(500);
        distantEnemy.RadiationDuration.Should().Be(0);
    }

    [Fact]
    public void TryMoveTower_WhenSupportStrikeSelected_IsRejected()
    {
        var simulation = new FrontlineGameSimulation(Path, 10);
        var support = FrontlineTowerCatalog.Find("suptower3")!;
        simulation.TryBuildTower(support.CreateDefinition(), new FrontlinePoint(50, 0));
        var tower = simulation.Towers.Single();

        var moved = simulation.TryMoveTower(tower.Id, new FrontlinePoint(100, 0));

        moved.Should().BeFalse();
        simulation.Funds.Should().Be(10);
        tower.Cell.Should().Be(new FrontlineGridPoint(1, 0));
    }

    [Fact]
    public void Advance_WhenNapalmShellImpacts_CreatesPersistentBurnAreaEffect()
    {
        var simulation = new FrontlineGameSimulation(Path, 0, random: new Random(1));
        var support = FrontlineTowerCatalog.Find("suptower1")!;
        simulation.TryBuildTower(
            support.CreateDefinition() with { ShotLimit = 1 },
            new FrontlinePoint(50, 0));
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("grunt", 1000, 0.01, 10));
        var enemy = simulation.Enemies.Single();

        for (var tick = 0; tick < 34; tick++)
            simulation.Advance();

        simulation.ActiveSupportProjectileCount.Should().Be(1);
        enemy.Health.Should().Be(1000);

        for (var tick = 0; tick < 120; tick++)
            simulation.Advance();

        simulation.ActiveSupportProjectileCount.Should().Be(0);
        simulation.ActiveAreaEffectCount.Should().Be(1);
        enemy.Health.Should().BeLessThan(950);
    }

    [Fact]
    public void TrySellTower_WhenSupportStrikeSelected_DoesNotRefundOrRemoveIt()
    {
        var simulation = new FrontlineGameSimulation(Path, 0);
        var support = FrontlineTowerCatalog.Find("suptower3")!;
        simulation.TryBuildTower(support.CreateDefinition(), new FrontlinePoint(50, 0));
        var tower = simulation.Towers.Single();

        var sold = simulation.TrySellTower(tower.Id);

        sold.Should().BeFalse();
        simulation.Towers.Should().ContainSingle();
    }

    [Fact]
    public void ApplyDamage_WithBurnProfile_AppliesTemperatureAndDamageOverTime()
    {
        var simulation = new FrontlineGameSimulation(Path, 0);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("grunt", 100, 1, 5));
        var enemy = simulation.Enemies.Single();

        simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(Burn: 170));
        simulation.Advance();

        enemy.Health.Should().Be(99.5);
        enemy.BurnDuration.Should().BeGreaterThan(0);
    }

    [Fact]
    public void Advance_WhenDamageOverTimeKillsEnemy_AwardsCreditsAndKillScore()
    {
        var simulation = new FrontlineGameSimulation(Path, 0);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("grunt", 0.1, 1, 10));
        var enemy = simulation.Enemies.Single();
        simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(Burn: 1000));

        simulation.Advance();

        simulation.Enemies.Should().BeEmpty();
        simulation.Funds.Should().Be(10);
        simulation.Score.Should().Be(20);
    }

    [Fact]
    public void ApplyDamage_WithPoisonAgainstMetalEnemy_DoesNotApplyPoison()
    {
        var simulation = new FrontlineGameSimulation(Path, 0);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("tank", 100, 1, 5, IsMetal: true));
        var enemy = simulation.Enemies.Single();

        simulation.ApplyDamage(enemy.Id, new FrontlineDamageProfile(Poison: 100));
        simulation.Advance();

        enemy.PoisonDuration.Should().Be(0);
        enemy.Health.Should().Be(100);
    }

    [Fact]
    public void Advance_WhenHomingMissileCrossesTargetBetweenTicks_RegistersCollision()
    {
        var path = new FrontlinePath(
        [
            new FrontlinePoint(90, 50),
            new FrontlinePoint(300, 50)
        ]);
        var simulation = new FrontlineGameSimulation(path, 0);
        var missile = new FrontlineTowerDefinition(
            "missile-test",
            0,
            100,
            80,
            1,
            ProjectileProfile: new FrontlineProjectileProfile(
                FrontlineProjectileKind.HomingMissile,
                Speed: 4,
                CollisionRadius: 6),
            DamageProfile: new FrontlineDamageProfile(DirectDamage: 80));
        simulation.TryBuildTower(missile, new FrontlinePoint(50, 50));
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("target", 500, 0.0001, 10));
        var enemy = simulation.Enemies.Single();

        simulation.Advance(100);
        simulation.Advance(0.001);
        simulation.Advance(8);
        simulation.Advance(8);

        enemy.Health.Should().Be(420);
        simulation.Projectiles.Should().BeEmpty();
    }

    [Fact]
    public void Advance_WhenEnemyReachesEndOfPath_AppliesLifeDamage()
    {
        var simulation = new FrontlineGameSimulation(Path, 0, 1000);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("fast", 100, 100, 5));

        simulation.Advance();

        simulation.LevelHealth.Should().Be(900);
        simulation.Status.Should().Be(FrontlineGameStatus.Planning);
    }

    [Fact]
    public void Advance_WhenLevelHealthReachesZero_SetsDefeat()
    {
        var simulation = new FrontlineGameSimulation(Path, 0, 100);
        simulation.SpawnEnemy(new FrontlineEnemyDefinition("fast", 100, 100, 5));

        simulation.Advance();

        simulation.Status.Should().Be(FrontlineGameStatus.Defeat);
    }
}