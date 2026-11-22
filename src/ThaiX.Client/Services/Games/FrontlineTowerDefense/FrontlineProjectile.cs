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
    double VerticalVelocityScale = 0.018,
    double? Lifetime = null);

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
        Target = target;
        Position = position;
        Damage = damage;
        Profile = profile;
        _velocityX = initialVelocityX;
        _velocityY = initialVelocityY;

        if (profile.Kind == FrontlineProjectileKind.BallisticShell)
        {
            var predictedPosition = PredictPosition(target, 40);
            var distance = predictedPosition.DistanceTo(position);
            VerticalVelocity = distance * profile.Speed * profile.VerticalVelocityScale;
        }
    }

    public long Id { get; }
    public long TargetId { get; }
    internal FrontlineEnemy Target { get; }
    public FrontlinePoint Position { get; private set; }
    public double Height { get; private set; }
    public FrontlineDamageProfile Damage { get; }
    public FrontlineProjectileProfile Profile { get; }
    internal FrontlineTower Owner { get; }
    internal bool IsExpired { get; private set; }

    internal bool Advance(FrontlineEnemy target, double elapsedUnits)
    {
        Position = new FrontlinePoint(
            Position.X + _velocityX * elapsedUnits,
            Position.Y + _velocityY * elapsedUnits);

        if (Profile.Kind == FrontlineProjectileKind.HomingMissile
            && (target.Health <= 0 || target.Position.X <= 0))
        {
            IsExpired = true;
            return false;
        }

        if (Profile.Kind == FrontlineProjectileKind.BallisticShell)
        {
            Height += VerticalVelocity * elapsedUnits;
            VerticalVelocity -= Profile.Gravity * elapsedUnits;
            if (Height < 0)
                return true;

            _elapsedUnits += elapsedUnits;
            if (Profile.Lifetime is double ballisticLifetime && _elapsedUnits > ballisticLifetime)
            {
                IsExpired = true;
                return false;
            }

            if (target.Health > 0 && VerticalVelocity >= -0.05)
                SetVelocity(PredictPosition(target, 25));

            return false;
        }

        _elapsedUnits += elapsedUnits;
        if (Profile.Lifetime is double trackingLifetime && _elapsedUnits > trackingLifetime)
        {
            IsExpired = true;
            return false;
        }

        if (target.Health > 0)
            SetVelocity(target.Position);

        const double targetCollisionRadius = 8;
        var collisionRadiusSquared = Profile.Kind == FrontlineProjectileKind.HomingMissile
            ? Profile.CollisionRadius * Profile.CollisionRadius
            : Profile.CollisionRadius * Profile.CollisionRadius
                + targetCollisionRadius * targetCollisionRadius;
        var offsetX = target.Position.X - Position.X;
        var offsetY = target.Position.Y - Position.Y;

        return offsetX * offsetX + offsetY * offsetY <= collisionRadiusSquared;
    }

    internal void Expire() => IsExpired = true;

    private static FrontlinePoint PredictPosition(FrontlineEnemy target, double lookAhead) =>
        target.PredictPosition(lookAhead);

    private void SetVelocity(FrontlinePoint targetPosition)
    {
        var deltaX = targetPosition.X - Position.X;
        var deltaY = targetPosition.Y - Position.Y;
        var distance = new FrontlinePoint(deltaX, deltaY).Length;
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
    private double _elapsedUnits;
    private double _velocityX;
    private double _velocityY;
}
