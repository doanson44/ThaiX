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
            var distance = Math.Hypot(
                predictedPosition.X - position.X,
                predictedPosition.Y - position.Y);
            VerticalVelocity = distance * profile.Speed * profile.VerticalVelocityScale;
            SetVelocity(PredictPosition(target, 25));
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
        if ((target.Health <= 0 && Profile.Kind != FrontlineProjectileKind.BallisticShell)
            || (Profile.Kind == FrontlineProjectileKind.HomingMissile && target.Position.X <= 0))
        {
            IsExpired = true;
            return false;
        }

        if (Profile.Kind == FrontlineProjectileKind.BallisticShell)
        {
            var remainingUnits = elapsedUnits;
            while (remainingUnits > 0)
            {
                var step = Math.Min(remainingUnits, 1);
                var nextHeight = Height + VerticalVelocity * step;
                if (nextHeight < 0)
                {
                    var impactUnits = VerticalVelocity >= 0
                        ? step
                        : Math.Clamp(Height / -VerticalVelocity, 0, step);
                    Position = new FrontlinePoint(
                        Position.X + _velocityX * impactUnits,
                        Position.Y + _velocityY * impactUnits);
                    Height = 0;
                    return true;
                }

                Position = new FrontlinePoint(
                    Position.X + _velocityX * step,
                    Position.Y + _velocityY * step);
                Height = nextHeight;
                VerticalVelocity -= Profile.Gravity * step;

                if (target.Health > 0 && VerticalVelocity >= -0.05)
                {
                    var predictedPosition = PredictPosition(target, 25);
                    SetVelocity(predictedPosition);
                }

                _elapsedUnits += step;
                if (Profile.Lifetime is double lifetime && _elapsedUnits > lifetime)
                {
                    IsExpired = true;
                    return false;
                }

                remainingUnits -= step;
            }

            return false;
        }

        var previousPosition = Position;
        Position = new FrontlinePoint(
            Position.X + _velocityX * elapsedUnits,
            Position.Y + _velocityY * elapsedUnits);
        _elapsedUnits += elapsedUnits;
        if (Profile.Lifetime is double lifetime && _elapsedUnits > lifetime)
        {
            IsExpired = true;
            return false;
        }

        SetVelocity(target.Position);

        const double targetCollisionRadius = 8;
        var collisionRadiusSquared = Profile.Kind == FrontlineProjectileKind.HomingMissile
            ? Profile.CollisionRadius * Profile.CollisionRadius
            : Profile.CollisionRadius * Profile.CollisionRadius
                + targetCollisionRadius * targetCollisionRadius;

        var segmentX = Position.X - previousPosition.X;
        var segmentY = Position.Y - previousPosition.Y;
        var segmentLengthSquared = segmentX * segmentX + segmentY * segmentY;
        var projection = segmentLengthSquared <= 0
            ? 0
            : Math.Clamp(
                ((target.Position.X - previousPosition.X) * segmentX
                    + (target.Position.Y - previousPosition.Y) * segmentY)
                    / segmentLengthSquared,
                0,
                1);
        var closestX = previousPosition.X + projection * segmentX;
        var closestY = previousPosition.Y + projection * segmentY;
        var offsetX = target.Position.X - closestX;
        var offsetY = target.Position.Y - closestY;

        return offsetX * offsetX + offsetY * offsetY <= collisionRadiusSquared;
    }

    internal void Expire() => IsExpired = true;

    private static FrontlinePoint PredictPosition(FrontlineEnemy target, double lookAhead)
    {
        var position = target.Position;
        var nextPosition = target.Path.PointAt(target.Distance + 1);
        var directionX = nextPosition.X - position.X;
        var directionY = nextPosition.Y - position.Y;
        var directionLength = Math.Hypot(directionX, directionY);

        if (directionLength <= 0)
            return position;

        return new FrontlinePoint(
            position.X + directionX / directionLength * lookAhead,
            position.Y + directionY / directionLength * lookAhead);
    }

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
    private double _elapsedUnits;
    private double _velocityX;
    private double _velocityY;
}
