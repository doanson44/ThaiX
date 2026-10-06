using Microsoft.AspNetCore.Components;
using ThaiX.Client.Services.Ui;

namespace ThaiX.Client.Layout;

/// <summary>
/// Base class for ThaiX pages and components that enforces IViewportService lifecycle
/// and IDisposable pattern. Inherit from this instead of ComponentBase for any page
/// or component that needs viewport awareness.
/// </summary>
public abstract class ThaiXPageBase : ComponentBase, IDisposable
{
    [Inject]
    protected IViewportService Viewport { get; set; } = default!;

    protected bool IsDisposed { get; private set; }

    protected override void OnInitialized()
    {
        Viewport.OnBreakpointChanged += OnBreakpointChanged;
        base.OnInitialized();
    }

    private void OnBreakpointChanged() => InvokeAsync(StateHasChanged);

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (IsDisposed) return;
        if (disposing)
        {
            Viewport.OnBreakpointChanged -= OnBreakpointChanged;
        }
        IsDisposed = true;
    }
}
