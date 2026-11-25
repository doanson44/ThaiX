using Microsoft.JSInterop;

namespace ThaiX.Client.Services.Games.FrontlineTowerDefense;

/// <summary>
/// Plays the legacy Frontline Tower Defense sound cues through the browser Audio elements.
/// The simulation only raises <see cref="FrontlineSound"/> values; all playback lives here.
/// </summary>
public sealed class FrontlineAudioService(IJSRuntime jsRuntime) : IAsyncDisposable
{
    private const string MenuMusicFile = "85";
    private const string LevelNoiseFile = "84";
    private const double MenuMusicVolume = 0.9;
    private const double LevelNoiseVolume = 0.6;

    private IJSObjectReference? _module;
    private string? _soundBaseUri;
    private bool _musicPlaying;
    private bool _noisePlaying;

    public bool IsMuted { get; private set; }

    public async Task InitializeAsync(string applicationBaseUri)
    {
        _soundBaseUri = new Uri(
            new Uri(applicationBaseUri, UriKind.Absolute),
            "games/frontline-tower-defense/assets/snd/").ToString();
        _module ??= await jsRuntime.InvokeAsync<IJSObjectReference>("import", "./js/frontlineAudio.js");
    }

    public Task PlayAsync(FrontlineSound sound) =>
        PlayFileAsync(GetFileName(sound));

    public async Task StartMenuMusicAsync()
    {
        _musicPlaying = true;
        await LoopFileAsync(MenuMusicFile, MenuMusicVolume);
    }

    public async Task StopMenuMusicAsync()
    {
        _musicPlaying = false;
        await StopFileAsync(MenuMusicFile);
    }

    public async Task StartLevelNoiseAsync()
    {
        _noisePlaying = true;
        await LoopFileAsync(LevelNoiseFile, LevelNoiseVolume);
    }

    public async Task StopLevelNoiseAsync()
    {
        _noisePlaying = false;
        await StopFileAsync(LevelNoiseFile);
    }

    public async Task ToggleMuteAsync()
    {
        await SetMutedAsync(!IsMuted);
    }

    public async Task SetMutedAsync(bool muted)
    {
        IsMuted = muted;

        if (_module is null)
            return;

        await _module.InvokeVoidAsync("setMuted", muted);

        if (muted)
            return;

        if (_musicPlaying)
            await LoopFileAsync(MenuMusicFile, MenuMusicVolume);
        if (_noisePlaying)
            await LoopFileAsync(LevelNoiseFile, LevelNoiseVolume);
    }

    private async Task PlayFileAsync(string file)
    {
        if (_module is null || _soundBaseUri is null)
            return;

        await _module.InvokeVoidAsync("play", BuildUri(file), 1);
    }

    private async Task LoopFileAsync(string file, double volume)
    {
        if (_module is null || _soundBaseUri is null)
            return;

        await _module.InvokeVoidAsync("loop", BuildUri(file), volume);
    }

    private async Task StopFileAsync(string file)
    {
        if (_module is null || _soundBaseUri is null)
            return;

        await _module.InvokeVoidAsync("stopLoop", BuildUri(file));
    }

    private string BuildUri(string file) => _soundBaseUri + file + ".mp3";

    private static string GetFileName(FrontlineSound sound) => sound switch
    {
        FrontlineSound.Rifle => "74",
        FrontlineSound.Shotgun => "70",
        FrontlineSound.Sniper => "68",
        FrontlineSound.Electro => "78",
        FrontlineSound.Flame => "80",
        FrontlineSound.MachineGun => "76",
        FrontlineSound.Bio => "72",
        FrontlineSound.Missile => "75",
        FrontlineSound.Laser => "79",
        FrontlineSound.Launcher => "71",
        FrontlineSound.Explode => "66",
        FrontlineSound.Splat => "83",
        _ => "66"
    };

    public async ValueTask DisposeAsync()
    {
        if (_module is null)
            return;

        try
        {
            await _module.InvokeVoidAsync("stopAll");
            await _module.DisposeAsync();
        }
        catch (JSDisconnectedException)
        {
        }
    }
}
