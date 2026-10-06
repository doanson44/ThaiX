using System.Collections.Concurrent;
using Microsoft.Playwright;
using Xunit;

namespace ThaiX.Client.VisualTests;

/// <summary>
/// Collection fixture: builds the client Docker image, starts the container (VisualTests / ForceDemo),
/// logs in once (storage state), then creates isolated browser contexts for parallel Playwright work.
/// </summary>
public sealed class AuthFixture : IAsyncLifetime
{
    /// <summary>Any credentials work in demo mode.</summary>
    private static readonly string Username = Environment.GetEnvironmentVariable("TEST_USERNAME") ?? "admin";
    private static readonly string Password = Environment.GetEnvironmentVariable("TEST_PASSWORD") ?? "P@ssw0rd";

    private const string LoginUsernameSelector =
        "input[name='username'], input[name='Username'], .rz-textbox[name='username'] input, input.rz-textbox";
    private const string LoginPasswordSelector =
        "input[name='password'], input[name='Password'], input[type='password']";

    private readonly ClientAppHost _appHost = new();
    private string? _authStorageStatePath;
    private string? _publicStorageStatePath;

    public string BaseUrl { get; private set; } = string.Empty;

    public IPlaywright Playwright { get; private set; } = null!;
    public IBrowser Browser { get; private set; } = null!;

    /// <summary>
    /// Max concurrent Playwright workers. Override with VISUAL_TEST_PARALLELISM (default 4).
    /// </summary>
    public static int MaxDegreeOfParallelism { get; } = ResolveParallelism();

    public async Task InitializeAsync()
    {
        await _appHost.StartAsync();
        BaseUrl = _appHost.BaseUrl;

        Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
        Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });

        var authContext = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 }
        });
        var authenticatedPage = await authContext.NewPageAsync();

        await GotoAsync(authenticatedPage, "/auth/login", 1280, 800);
        await EnsureDemoModeAsync(authenticatedPage);

        await authenticatedPage.FillAsync(LoginUsernameSelector, Username);
        await authenticatedPage.FillAsync(LoginPasswordSelector, Password);
        await authenticatedPage.ClickAsync("button[type='submit']");
        await authenticatedPage.WaitForURLAsync(
            url => new Uri(url).AbsolutePath == "/",
            new PageWaitForURLOptions { Timeout = 60000 });
        await WaitForPageReadyAsync(authenticatedPage, "/");
        await EnsureDemoModeAsync(authenticatedPage);

        _authStorageStatePath = Path.Combine(Path.GetTempPath(), $"ThaiX-visualtests-auth-{Guid.NewGuid():N}.json");
        await authContext.StorageStateAsync(new BrowserContextStorageStateOptions { Path = _authStorageStatePath });
        await authContext.CloseAsync();

        var publicContext = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 }
        });
        var publicPage = await publicContext.NewPageAsync();
        await GotoAsync(publicPage, "/auth/login", 1280, 800);
        await EnsureDemoModeAsync(publicPage);

        _publicStorageStatePath = Path.Combine(Path.GetTempPath(), $"ThaiX-visualtests-public-{Guid.NewGuid():N}.json");
        await publicContext.StorageStateAsync(new BrowserContextStorageStateOptions { Path = _publicStorageStatePath });
        await publicContext.CloseAsync();
    }

    /// <summary>
    /// Opens an isolated browser context + page (safe for parallel workers). Caller must dispose the context.
    /// </summary>
    public async Task<(IBrowserContext Context, IPage Page)> CreatePageAsync(
        bool authenticated, int width, int height)
    {
        var storagePath = authenticated ? _authStorageStatePath : _publicStorageStatePath;
        if (string.IsNullOrEmpty(storagePath))
        {
            throw new InvalidOperationException("AuthFixture storage state was not initialized.");
        }

        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            StorageStatePath = storagePath,
            ViewportSize = new ViewportSize { Width = width, Height = height }
        });
        var page = await context.NewPageAsync();
        return (context, page);
    }

    /// <summary>
    /// Runs <paramref name="work"/> over <paramref name="items"/> with bounded parallelism.
    /// Collects failures and asserts once at the end.
    /// </summary>
    public async Task RunParallelAsync<T>(
        IReadOnlyCollection<T> items,
        Func<T, IPage, Task> work,
        Func<T, bool> requiresAuth,
        Func<T, (int Width, int Height)> viewport,
        Func<T, string> describe)
    {
        var failures = new ConcurrentBag<string>();

        await Parallel.ForEachAsync(
            items,
            new ParallelOptions { MaxDegreeOfParallelism = MaxDegreeOfParallelism },
            async (item, _) =>
            {
                var (width, height) = viewport(item);
                var (context, page) = await CreatePageAsync(requiresAuth(item), width, height);
                try
                {
                    await work(item, page);
                }
                catch (Exception ex)
                {
                    failures.Add($"{describe(item)}: {ex.Message}");
                }
                finally
                {
                    await context.CloseAsync();
                }
            });

        Assert.True(
            failures.IsEmpty,
            $"Parallel visual checks failed ({failures.Count}/{items.Count}):{Environment.NewLine}" +
            string.Join(Environment.NewLine, failures.OrderBy(x => x)));
    }

    /// <summary>
    /// Navigates and waits until Blazor has replaced the boot splash and route UI is interactive.
    /// </summary>
    public async Task GotoAsync(IPage page, string route, int width, int height, int settleMs = 300)
    {
        if (page.ViewportSize is null || page.ViewportSize.Width != width || page.ViewportSize.Height != height)
        {
            await page.SetViewportSizeAsync(width, height);
        }

        await page.GotoAsync($"{BaseUrl}{route}", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = 120000
        });

        await WaitForPageReadyAsync(page, route);

        if (settleMs > 0)
        {
            await page.WaitForTimeoutAsync(settleMs);
        }
    }

    /// <summary>
    /// Boot splash (.loading-progress) must be gone; then wait for route-specific UI.
    /// Full page reloads re-download WASM — this often takes tens of seconds in Docker.
    /// </summary>
    public static async Task WaitForPageReadyAsync(IPage page, string route)
    {
        var loading = page.Locator("svg.loading-progress");
        try
        {
            await loading.First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Detached,
                Timeout = 180000
            });
        }
        catch (TimeoutException ex)
        {
            throw new TimeoutException(
                $"Blazor boot splash still visible at {page.Url} after 180s (route '{route}').",
                ex);
        }

        var errorUi = page.Locator("#blazor-error-ui");
        if (await errorUi.IsVisibleAsync())
        {
            throw new InvalidOperationException($"Blazor error UI is visible at {page.Url}.");
        }

        await WaitForRouteContentAsync(page, route);

        try
        {
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle, new PageWaitForLoadStateOptions { Timeout = 30000 });
        }
        catch (TimeoutException)
        {
            // Some pages keep websockets/polling; content wait above is enough.
        }
    }

    private static async Task WaitForRouteContentAsync(IPage page, string route)
    {
        var path = route.Split('?', 2)[0];

        if (path.Equals("/auth/external-login-callback", StringComparison.OrdinalIgnoreCase))
        {
            await page.Locator(".rz-alert, .rz-progressbar-circular, a[href='/auth/login'], .rz-link, form, .rz-card")
                .First
                .WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 60000
                });
            return;
        }

        if (path.Equals("/auth/login", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/auth/login-2fa", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/auth/register", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/auth/forgot-password", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/auth/reset-password", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/auth/confirm-email", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/auth/resend-confirmation", StringComparison.OrdinalIgnoreCase) ||
            path.Equals("/auth/lockout", StringComparison.OrdinalIgnoreCase))
        {
            await page.Locator($"{LoginUsernameSelector}, {LoginPasswordSelector}, form, .rz-card, .rz-alert, .rz-text, h1, h2, h3, h4")
                .First
                .WaitForAsync(new LocatorWaitForOptions
                {
                    State = WaitForSelectorState.Visible,
                    Timeout = 60000
                });
            return;
        }

        var shell = page.Locator(".rz-body, .rz-layout, .rz-panel-menu, main, .rz-card, h1, h2, h3, h4, h5, h6").First;
        try
        {
            await shell.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 60000
            });
        }
        catch (TimeoutException)
        {
            await page.Locator("#app >> visible=true").First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30000
            });
        }
    }

    private static async Task EnsureDemoModeAsync(IPage page)
    {
        var requireDemo = !string.Equals(
            Environment.GetEnvironmentVariable("ALLOW_REAL_API"),
            "true",
            StringComparison.OrdinalIgnoreCase);

        if (!requireDemo)
        {
            return;
        }

        var banner = page.Locator("text=/Demo mode/i");
        try
        {
            await banner.First.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 30000
            });
        }
        catch (TimeoutException)
        {
            throw new InvalidOperationException(
                "Visual tests expect demo mode, but the Demo mode banner was not found. " +
                "Ensure VisualTests / ForceDemo host is used, or set ALLOW_REAL_API=true.");
        }
    }

    private static int ResolveParallelism()
    {
        var raw = Environment.GetEnvironmentVariable("VISUAL_TEST_PARALLELISM");
        if (int.TryParse(raw, out var configured) && configured > 0)
        {
            return configured;
        }

        return Math.Clamp(Environment.ProcessorCount, 2, 6);
    }

    public async Task DisposeAsync()
    {
        if (Browser is not null)
        {
            await Browser.CloseAsync();
        }

        Playwright?.Dispose();
        await _appHost.DisposeAsync();

        TryDelete(_authStorageStatePath);
        TryDelete(_publicStorageStatePath);
    }

    private static void TryDelete(string? path)
    {
        if (string.IsNullOrEmpty(path))
        {
            return;
        }

        try
        {
            File.Delete(path);
        }
        catch
        {
            // Best-effort temp cleanup.
        }
    }
}

[CollectionDefinition("ThaiX Playwright")]
public sealed class PlaywrightCollection : ICollectionFixture<AuthFixture>;
