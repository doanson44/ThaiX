namespace ThaiX.Client.Services.Ui;

public interface IViewportService
{
    bool IsMobile { get; }
    bool IsTablet { get; }
    bool IsDesktop { get; }
    int Width { get; }

    /// <summary>
    /// Fired when the active breakpoint (Mobile, Tablet, Desktop) changes.
    /// </summary>
    event Action? OnBreakpointChanged;

    /// <summary>
    /// Must be called from a component's OnAfterRenderAsync to initialize JS interop.
    /// </summary>
    Task InitializeAsync();
}
