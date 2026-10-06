using Microsoft.JSInterop;

namespace ThaiX.Client.Services.Ui;

public sealed class AppThemeService : IThemeService
{
    private readonly IJSRuntime _js;
    private bool _initialized;

    public AppThemeService(IJSRuntime js)
    {
        _js = js;
    }

    public AppTheme Current { get; private set; } = AppTheme.Dark;

    public event Action? OnChanged;

    public async Task InitializeAsync()
    {
        if (_initialized) return;

        try
        {
            var value = await _js.InvokeAsync<string>("ThaiXTheme.get");
            Current = Parse(value);
        }
        catch
        {
            Current = AppTheme.Dark;
        }

        _initialized = true;
        OnChanged?.Invoke();
    }

    public async Task SetThemeAsync(AppTheme theme)
    {
        Current = theme;
        try
        {
            await _js.InvokeAsync<string>("ThaiXTheme.set", ToStorage(theme));
        }
        catch
        {
            /* JS unavailable during prerender / early startup */
        }

        OnChanged?.Invoke();
    }

    private static AppTheme Parse(string? value) =>
        string.Equals(value, "light", StringComparison.OrdinalIgnoreCase)
            ? AppTheme.Light
            : AppTheme.Dark;

    private static string ToStorage(AppTheme theme) =>
        theme == AppTheme.Light ? "light" : "dark";
}
