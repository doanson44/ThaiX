using Microsoft.JSInterop;

namespace ThaiX.Client.Services.Ui;

public sealed class ViewportService : IViewportService, IAsyncDisposable
{
    private readonly IJSRuntime _jsRuntime;
    private DotNetObjectReference<ViewportService>? _dotNetRef;
    private IJSObjectReference? _module;
    private bool _initialized;

    public bool IsMobile { get; private set; }
    public bool IsTablet { get; private set; }
    public bool IsDesktop { get; private set; }
    public int Width { get; private set; }

    public event Action? OnBreakpointChanged;

    public ViewportService(IJSRuntime jsRuntime)
    {
        _jsRuntime = jsRuntime;
    }

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        _dotNetRef = DotNetObjectReference.Create(this);
        var jsCode = @"
        export function observeResize(dotNetRef) {
            const onResize = () => {
                dotNetRef.invokeMethodAsync('OnWindowResize', window.innerWidth);
            };
            window.addEventListener('resize', onResize);
            return window.innerWidth;
        }";

        try
        {
            var bytes = System.Text.Encoding.UTF8.GetBytes(jsCode);
            var base64 = Convert.ToBase64String(bytes);

            // We use JS module via Data URI to avoid polluting the global scope or needing a custom .js file
            _module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import", $"data:text/javascript;base64,{base64}");

            var width = await _module.InvokeAsync<int>("observeResize", _dotNetRef);
            OnWindowResize(width);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Failed to initialize ViewportService: {ex.Message}");
            // default fallback if JS fails (e.g. server-side rendering or missing JS interop access)
            OnWindowResize(1024);
        }
        _initialized = true;
    }

    [JSInvokable]
    public void OnWindowResize(int width)
    {
        var wasMobile = IsMobile;
        var wasTablet = IsTablet;
        var wasDesktop = IsDesktop;

        Width = width;
        IsMobile = width < 768;
        IsTablet = width >= 768 && width < 1024;
        IsDesktop = width >= 1024;

        if (wasMobile != IsMobile || wasTablet != IsTablet || wasDesktop != IsDesktop)
        {
            OnBreakpointChanged?.Invoke();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
