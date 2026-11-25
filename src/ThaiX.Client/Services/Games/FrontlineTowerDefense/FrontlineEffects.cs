namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

/// <summary>
/// A horizontal sprite strip from the legacy Flump library. <see cref="Size"/> is the
/// square frame size and <see cref="FrameCount"/> the number of frames packed in the strip.
/// </summary>
public sealed record FrontlineSpriteStrip(string Symbol, double Size, int FrameCount)
{
    public static readonly FrontlineSpriteStrip Flame = new("flame1", 16, 8);
    public static readonly FrontlineSpriteStrip Poison = new("rad1", 16, 8);
    public static readonly FrontlineSpriteStrip Radiation = new("freeze1", 16, 8);
    public static readonly FrontlineSpriteStrip Freeze = new("iced1", 16, 8);
    public static readonly FrontlineSpriteStrip Blood = new("bloodSplatter", 16, 8);
    public static readonly FrontlineSpriteStrip MuzzleFlash = new("muzzleFlash", 16, 8);
    public static readonly FrontlineSpriteStrip Trail = new("mtrail", 16, 10);
    public static readonly FrontlineSpriteStrip Smoke = new("smoke1", 64, 10);
    public static readonly FrontlineSpriteStrip Explosion = new("explosion1", 64, 16);
    public static readonly FrontlineSpriteStrip FireExplosion = new("fExplode", 64, 16);
    public static readonly FrontlineSpriteStrip BlueFlash = new("blueFlash", 16, 8);
    public static readonly FrontlineSpriteStrip LaserBlast = new("laserB", 16, 8);
    public static readonly FrontlineSpriteStrip Shell = new("shell1", 32, 10);
    public static readonly FrontlineSpriteStrip PlasmaShell = new("pshell1", 32, 10);
    public static readonly FrontlineSpriteStrip BioShot = new("bioFX", 32, 10);
    public static readonly FrontlineSpriteStrip FlameShot = new("flamer", 32, 10);
    public static readonly FrontlineSpriteStrip BioCloud = new("bioCloud1", 32, 10);
    public static readonly FrontlineSpriteStrip Shadow = new("shadow1", 32, 10);
    public static readonly FrontlineSpriteStrip Crater = new("crater1", 32, 4);
    public static readonly FrontlineSpriteStrip BloodDecal = new("blood1", 16, 4);
    public static readonly FrontlineSpriteStrip Scorch = new("scorch1", 32, 4);
}

/// <summary>
/// A legacy <c>renderSprite</c> effect particle. The frame advances at
/// <see cref="FrameRate"/> frames per simulation unit and the particle expires once the
/// accumulated life passes the lifespan, matching the original runtime.
/// </summary>
public sealed class FrontlineParticle
{
    internal FrontlineParticle(
        FrontlineSpriteStrip strip,
        FrontlinePoint position,
        double velocityX,
        double velocityY,
        double frameRate,
        double lifeSpan,
        bool glow)
    {
        Strip = strip;
        X = position.X;
        Y = position.Y;
        VelocityX = velocityX;
        VelocityY = velocityY;
        FrameRate = frameRate;
        LifeSpan = lifeSpan;
        Glow = glow;
    }

    public FrontlineSpriteStrip Strip { get; }
    public double X { get; private set; }
    public double Y { get; private set; }
    public int Frame { get; private set; }
    public bool Glow { get; }
    private double VelocityX { get; }
    private double VelocityY { get; }
    private double FrameRate { get; }
    private double LifeSpan { get; }
    private double Life { get; set; }
    private double _frameTime;

    internal bool Advance(double elapsedUnits)
    {
        X += VelocityX * elapsedUnits;
        Y += VelocityY * elapsedUnits;
        Life += FrameRate * elapsedUnits;
        _frameTime += FrameRate * elapsedUnits;
        Frame = (int)Math.Floor(_frameTime);
        if (Frame >= Strip.FrameCount)
        {
            _frameTime = 0;
            Frame = 0;
        }

        return Life <= LifeSpan;
    }
}

/// <summary>A permanent ground decal copied from a legacy decal strip.</summary>
public sealed record FrontlineDecal(FrontlinePoint Position, FrontlineSpriteStrip Strip, int Frame);

/// <summary>A legacy <c>Spark</c> hit effect drawn by the FX layer.</summary>
public sealed class FrontlineSpark
{
    internal FrontlineSpark(
        FrontlinePoint position,
        int life,
        double velocityX,
        double velocityY,
        bool fade)
    {
        X = position.X;
        Y = position.Y;
        Life = life;
        VelocityX = velocityX;
        VelocityY = velocityY;
        Fade = fade;
    }

    public double X { get; private set; }
    public double Y { get; private set; }
    public double Life { get; private set; }
    public double VelocityX { get; private set; }
    public double VelocityY { get; private set; }
    public bool Fade { get; }

    internal bool Advance()
    {
        Life--;
        X += VelocityX;
        Y += VelocityY;
        if (Fade)
        {
            VelocityY += 0.1;
            VelocityX *= 0.99;
        }

        return Life > 0;
    }
}
