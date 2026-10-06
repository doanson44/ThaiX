namespace ThaiX.Client.Services.Ui;

public enum AppTheme
{
    Dark,
    Light
}

public interface IThemeService
{
    AppTheme Current { get; }

    event Action? OnChanged;

    Task InitializeAsync();

    Task SetThemeAsync(AppTheme theme);
}
