using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.Localization;
using ThaiX.Client.Localization;

namespace ThaiX.Client.Pages.Games;

public readonly record struct GameResult(decimal Score, int DurationSeconds, string? Mode, bool Won = false);

/// <summary>
/// Shared lifecycle for the game boards hosted by <see cref="GamePlayer"/>: restart, pause-aware
/// clock, keyboard and swipe input, the tick loop, and reporting the final score.
/// </summary>
public abstract class GameBase : ComponentBase, IDisposable
{
    private const double SwipeThreshold = 25;

    private CancellationTokenSource? _loop;
    private DateTime _startedAt;
    private DateTime? _pausedAt;
    private (double X, double Y)? _swipeStart;

    [Inject] protected IStringLocalizer<SharedResource> L { get; set; } = default!;

    [Parameter] public bool IsPaused { get; set; }
    [Parameter] public EventCallback<GameResult> OnCompleted { get; set; }

    protected bool Finished { get; private set; }
    protected bool CanPlay => !IsPaused && !Finished;

    protected int ElapsedSeconds =>
        Math.Max(1, (int)Math.Round(((_pausedAt ?? DateTime.UtcNow) - _startedAt).TotalSeconds));

    /// <summary>Sets up a fresh round. Called on first render and on every restart.</summary>
    protected abstract void Reset();

    /// <summary>Handles a pressed key. <paramref name="key"/> is the lower-cased DOM key name.</summary>
    protected virtual void OnKey(string key)
    {
    }

    protected virtual void OnKeyUp(string key)
    {
    }

    protected override void OnInitialized() => Start();

    protected override void OnParametersSet()
    {
        if (IsPaused)
        {
            _pausedAt ??= DateTime.UtcNow;
        }
        else if (_pausedAt is { } pausedAt)
        {
            _startedAt += DateTime.UtcNow - pausedAt;
            _pausedAt = null;
        }
    }

    public void Restart()
    {
        Start();
        StateHasChanged();
    }

    public void HandleKey(string key, bool released = false)
    {
        // Releases are always delivered so a key held across a pause does not stay "pressed".
        key = key.ToLowerInvariant();
        if (released)
            OnKeyUp(key);
        else if (CanPlay)
            OnKey(key);
        else
            return;

        StateHasChanged();
    }

    protected void RestartClock()
    {
        _startedAt = DateTime.UtcNow;
        _pausedAt = IsPaused ? _startedAt : null;
    }

    protected void Finish(decimal score, string? mode = null, bool won = false)
    {
        if (Finished)
            return;

        Finished = true;
        StopLoop();
        _ = OnCompleted.InvokeAsync(new GameResult(score, ElapsedSeconds, mode, won));
    }

    /// <summary>Runs <paramref name="tick"/> on a fixed interval while the game is playable.</summary>
    protected void StartLoop(int intervalMilliseconds, Action tick)
    {
        StopLoop();
        _loop = new CancellationTokenSource();
        _ = RunLoopAsync(intervalMilliseconds, tick, _loop.Token);
    }

    protected void SwipeStart(PointerEventArgs e) => _swipeStart = (e.ClientX, e.ClientY);

    /// <summary>Translates a finished swipe into the matching arrow key.</summary>
    protected void SwipeEnd(PointerEventArgs e)
    {
        if (_swipeStart is not { } start)
            return;

        _swipeStart = null;
        var dx = e.ClientX - start.X;
        var dy = e.ClientY - start.Y;
        if (!CanPlay || Math.Max(Math.Abs(dx), Math.Abs(dy)) < SwipeThreshold)
            return;

        OnKey(Math.Abs(dx) > Math.Abs(dy)
            ? dx > 0 ? "arrowright" : "arrowleft"
            : dy > 0 ? "arrowdown" : "arrowup");
    }

    /// <summary>Formats an SVG coordinate independently of the UI culture.</summary>
    protected static string Px(double value) => value.ToString("0.##", CultureInfo.InvariantCulture);

    public void Dispose()
    {
        StopLoop();
        GC.SuppressFinalize(this);
    }

    private void Start()
    {
        StopLoop();
        Finished = false;
        _swipeStart = null;
        RestartClock();
        Reset();
    }

    private void StopLoop()
    {
        _loop?.Cancel();
        _loop = null;
    }

    private async Task RunLoopAsync(int intervalMilliseconds, Action tick, CancellationToken cancellationToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(intervalMilliseconds));

        try
        {
            while (await timer.WaitForNextTickAsync(cancellationToken))
            {
                await InvokeAsync(() =>
                {
                    if (cancellationToken.IsCancellationRequested || !CanPlay)
                        return;

                    tick();
                    StateHasChanged();
                });
            }
        }
        catch (OperationCanceledException)
        {
        }
    }
}
