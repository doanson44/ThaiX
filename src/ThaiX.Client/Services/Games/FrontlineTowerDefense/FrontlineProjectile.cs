namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public enum FrontlineProjectileKind
{
    HomingMissile,
    BallisticShell,
    TrackingPoison,
    TrackingFlame
}

public sealed record FrontlineProjectileProfile(
    FrontlineProjectileKind Kind,
    double Speed,
    double CollisionRadius,
    double SplashRadius = 0,
    double SplashDamageScale = 1,
    double StartOffset = 20,
    double VerticalOffset = 0,
    double Gravity = 0.09,
    double VerticalVelocityScale = 0.018);

public sealed class FrontlineProjectile
{
    internal FrontlineProjectile(
        long id,
        FrontlineTower owner,
        FrontlineEnemy target,
        FrontlinePoint position,
        FrontlineDamageProfile damage,
        FrontlineProjectileProfile profile,
        double initialVelocityX = 0,
        double initialVelocityY = 0)
    {
        Id = id;
        Owner = owner;
        TargetId = target.Id;
        Position = position;
        Damage = damage;
        Profile = profile;
        _velocityX = initialVelocityX;
        _velocityY = initialVelocityY;

        if (profile.Kind == FrontlineProjectileKind.BallisticShell)
        {
            var predictedPosition = target.Path.PointAt(
                target.Distance + target.Definition.Speed * target.MovementMultiplier * 40);
            var distance = Math.Hypot(
                predictedPosition.X - position.X,
                predictedPosition.Y - position.Y);
            VerticalVelocity = distance * profile.Speed * profile.VerticalVelocityScale;
        }
    }

    public long Id { get; }
    public long TargetId { get; }
    public FrontlinePoint Position { get; private set; }
    public double Height { get; private set; }
    public FrontlineDamageProfile Damage { get; }
    public FrontlineProjectileProfile Profile { get; }
    internal FrontlineTower Owner { get; }
    internal bool IsExpired { get; private set; }

    internal bool Advance(FrontlineEnemy target, double elapsedUnits)
    {
        if (target.Health <= 0
            || (Profile.Kind == FrontlineProjectileKind.HomingMissile && target.Position.X <= 0))
        {
            IsExpired = true;
            return false;
        }

        if (Profile.Kind == FrontlineProjectileKind.BallisticShell)
        {
            Position = new FrontlinePoint(
                Position.X + _velocityX * elapsedUnits,
                Position.Y + _velocityY * elapsedUnits);
            Height += VerticalVelocity * elapsedUnits;
            VerticalVelocity -= Profile.Gravity * elapsedUnits;

            if (VerticalVelocity >= -0.05)
            {
                var predictedPosition = target.Path.PointAt(
                    target.Distance + target.Definition.Speed * target.MovementMultiplier * 25);
                SetVelocity(predictedPosition);
            }

            return Height < 0;
        }

        Position = new FrontlinePoint(
            Position.X + _velocityX * elapsedUnits,
            Position.Y + _velocityY * elapsedUnits);

        SetVelocity(target.Position);

        var distance = Math.Hypot(
            target.Position.X - Position.X,
            target.Position.Y - Position.Y);
        var collisionDistance = Profile.Kind == FrontlineProjectileKind.HomingMissile
            ? Profile.CollisionRadius
            : Math.Sqrt(Profile.CollisionRadius * Profile.CollisionRadius + 36);

        return distance <= collisionDistance;
    }

    internal void Expire() => IsExpired = true;

    private void SetVelocity(FrontlinePoint targetPosition)
    {
        var deltaX = targetPosition.X - Position.X;
        var deltaY = targetPosition.Y - Position.Y;
        var distance = Math.Hypot(deltaX, deltaY);
        if (distance <= 0)
        {
            _velocityX = 0;
            _velocityY = 0;
            return;
        }

        _velocityX = deltaX / distance * Profile.Speed;
        _velocityY = deltaY / distance * Profile.Speed;
    }

    private double VerticalVelocity { get; set; }
    private double _velocityX;
    private double _velocityY;
}
