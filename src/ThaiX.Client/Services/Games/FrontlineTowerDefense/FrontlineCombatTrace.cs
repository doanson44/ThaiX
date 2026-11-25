namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

public enum FrontlineCombatTraceKind
{
    Bullet,
    Sniper,
    Shotgun,
    Lightning,
    EnergyBeam
}

public sealed class FrontlineCombatTrace(
    FrontlinePoint start,
    FrontlinePoint end,
    FrontlineCombatTraceKind kind,
    double life,
    double weight = 1)
{
    public FrontlinePoint Start { get; } = start;
    public FrontlinePoint End { get; } = end;
    public FrontlineCombatTraceKind Kind { get; } = kind;
    /// <summary>Legacy tracer line thickness multiplier.</summary>
    public double Weight { get; } = weight;
    public double LifeRemaining { get; private set; } = life;

    internal bool Advance()
    {
        LifeRemaining -= Kind == FrontlineCombatTraceKind.Lightning ? 3 : 1;
        return LifeRemaining > 0;
    }
}
