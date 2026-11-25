namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

/// <summary>
/// Tracks the six legacy instruction dialogs. The original instruction manager lives on the
/// persistent game timeline, so the progress survives mission changes inside one page session.
/// </summary>
public sealed class FrontlineTutorialTracker
{
    public const int StepCount = 6;

    private readonly bool[] _seen = new bool[StepCount];

    /// <summary>Step currently shown, or <c>null</c> when no dialog is open.</summary>
    public int? Current { get; private set; }

    public bool IsFinished { get; private set; }

    /// <summary>
    /// Returns the dialog to display. Steps 1 and 2 have no close button: the player advances
    /// them by buying and placing a tower, and a later step replaces an open dialog exactly
    /// like the legacy instruction manager chain.
    /// </summary>
    public int? Evaluate(FrontlineTower? selectedTower, int towerCount, int enemyCount, int score)
    {
        if (IsFinished)
            return null;

        var next = FindStep(selectedTower, towerCount, enemyCount, score);
        if (next is null)
            return Current;

        _seen[next.Value] = true;
        Current = next;
        return next;
    }

    private int? FindStep(
        FrontlineTower? selectedTower,
        int towerCount,
        int enemyCount,
        int score)
    {
        for (var step = 0; step < StepCount; step++)
        {
            if (_seen[step] || !IsConditionMet(step, selectedTower, towerCount, enemyCount, score))
                continue;

            return step;
        }

        return null;
    }

    /// <summary>Restarts the dialogs, as the legacy manager is recreated on every level entry.</summary>
    public void Reset()
    {
        Array.Clear(_seen);
        Current = null;
        IsFinished = false;
    }

    public void Close()
    {
        if (Current is null)
            return;

        Current = null;
        if (_seen[StepCount - 1])
            IsFinished = true;
    }

    public static bool CanClose(int step) => step >= 2;

    private bool IsConditionMet(
        int step,
        FrontlineTower? selectedTower,
        int towerCount,
        int enemyCount,
        int score) => step switch
        {
            0 => towerCount <= 0,
            1 => towerCount > 0,
            2 => selectedTower is not null && !selectedTower.Definition.IsSupport && !selectedTower.IsHeld,
            3 => enemyCount <= 0 && score > 0,
            4 => enemyCount >= 10,
            5 => _seen[4] && enemyCount <= 3,
            _ => false
        };
}
