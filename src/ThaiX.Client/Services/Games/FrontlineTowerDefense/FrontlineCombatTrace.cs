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
    double life)
{
    public FrontlinePoint Start { get; } = start;
    public FrontlinePoint End { get; } = end;
    public FrontlineCombatTraceKind Kind { get; } = kind;
    public double LifeRemaining { get; private set; } = life;

    internal bool Advance()
    {
        LifeRemaining -= Kind == FrontlineCombatTraceKind.Lightning ? 3 : 1;
        return LifeRemaining > 0;
    }
}
