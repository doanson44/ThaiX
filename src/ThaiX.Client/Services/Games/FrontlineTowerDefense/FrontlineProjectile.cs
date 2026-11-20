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
        FrontlineProjectileProfile profile)
    {
        Id = id;
        Owner = owner;
        TargetId = target.Id;
        Position = position;
        Damage = damage;
        Profile = profile;

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
        if (target.Health <= 0)
        {
            IsExpired = true;
            return false;
        }

        if (Profile.Kind == FrontlineProjectileKind.BallisticShell)
        {
            Position = new FrontlinePoint(
                Position.X + _velocityX * elapsedUnits,
                Position.Y + _velocityY * elapsedUnits);

            if (VerticalVelocity >= -0.05)
            {
                var predictedPosition = target.Path.PointAt(
                    target.Distance + target.Definition.Speed * target.MovementMultiplier * 40);
                var horizontalX = predictedPosition.X - Position.X;
                var horizontalY = predictedPosition.Y - Position.Y;
                var horizontalDistance = Math.Hypot(horizontalX, horizontalY);
                if (horizontalDistance <= Profile.CollisionRadius + 6)
                    return true;

                if (horizontalDistance > 0)
                {
                    _velocityX = horizontalX / horizontalDistance * Profile.Speed;
                    _velocityY = horizontalY / horizontalDistance * Profile.Speed;
                }
            }

            Height += VerticalVelocity * elapsedUnits;
            VerticalVelocity -= Profile.Gravity * elapsedUnits;
            return Height < 0;
        }

        var deltaX = target.Position.X - Position.X;
        var deltaY = target.Position.Y - Position.Y;
        var distance = Math.Hypot(deltaX, deltaY);
        var impactDistance = Math.Sqrt(
            Profile.CollisionRadius * Profile.CollisionRadius + 36);

        if (distance <= impactDistance)
            return true;

        if (distance <= 0)
            return true;

        var travelDistance = Math.Min(distance, Profile.Speed * elapsedUnits);
        Position = new FrontlinePoint(
            Position.X + deltaX / distance * travelDistance,
            Position.Y + deltaY / distance * travelDistance);
        return Math.Hypot(
            target.Position.X - Position.X,
            target.Position.Y - Position.Y) <= impactDistance;
    }

    internal void Expire() => IsExpired = true;

    private double VerticalVelocity { get; set; }
    private double _velocityX;
    private double _velocityY;
}
