using Microsoft.Playwright;
using System.Collections.Concurrent;
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

    private static readonly object LogGate = new();
    private static readonly string LogFilePath = Path.Combine(
        Directory.GetCurrentDirectory(),
        "TestResults",
        "visual-tests.log");

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
        try
        {
            LogFixture("Starting the VisualTests application host.");
            await _appHost.StartAsync();
            BaseUrl = _appHost.BaseUrl;
            LogFixture($"VisualTests application host is HTTP-ready at {BaseUrl}.");

            LogFixture("Installing or validating the Playwright Chromium headless shell.");
            var browserInstallExitCode = Microsoft.Playwright.Program.Main(
                new[] { "install", "chromium", "--only-shell" });
            if (browserInstallExitCode != 0)
            {
                throw new InvalidOperationException(
                    $"Playwright Chromium installation failed with exit code {browserInstallExitCode}.");
            }

            LogFixture("Playwright Chromium headless shell is ready.");
            LogFixture("Creating Playwright and launching the shared Chromium browser.");
            Playwright = await Microsoft.Playwright.Playwright.CreateAsync();
            Browser = await Playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            Browser.Disconnected += (_, _) => LogFixture("Browser disconnected from the Playwright driver.");

            await VerifyBrowserStartupAsync();

            if (string.Equals(
                    Environment.GetEnvironmentVariable("VISUAL_SKIP_AUTH_LOGIN"),
                    "true",
                    StringComparison.OrdinalIgnoreCase))
            {
                await CreatePublicOnlyStorageStateAsync();
                return;
            }

            await CreateAuthenticationStorageStateAsync();
            await CreatePublicStorageStateAsync();
        }
        catch (Exception ex)
        {
            LogFixture($"Visual test fixture initialization failed: {ex}");
            try
            {
                await DisposeAsync();
            }
            catch (Exception cleanupException)
            {
                LogFixture($"Fixture cleanup after initialization failure also failed: {cleanupException}");
            }

            throw;
        }
    }

    private async Task VerifyBrowserStartupAsync()
    {
        LogFixture("Starting browser smoke check against the home page.");
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 }
        });

        try
        {
            var page = await context.NewPageAsync();
            AttachBrowserDiagnostics(page);
            await GotoAsync(page, "/", 1280, 800, settleMs: 0, networkIdleTimeoutMs: 0);
            await EnsureDemoModeAsync(page);
            LogFixture("Browser smoke check passed: home page rendered and Demo Mode was verified.");
        }
        catch (Exception ex)
        {
            LogFixture($"Browser smoke check failed: {ex}");
            throw new InvalidOperationException(
                "VisualTests application passed HTTP readiness but failed the browser startup smoke check.",
                ex);
        }
        finally
        {
            await context.CloseAsync();
        }
    }

    private async Task CreatePublicOnlyStorageStateAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 }
        });

        try
        {
            var page = await context.NewPageAsync();
            AttachBrowserDiagnostics(page);
            await GotoAsync(page, "/auth/login", 1280, 800);
            await EnsureDemoModeAsync(page);

            _publicStorageStatePath = Path.Combine(
                Path.GetTempPath(),
                $"ThaiX-visualtests-public-{Guid.NewGuid():N}.json");
            await context.StorageStateAsync(new BrowserContextStorageStateOptions
            {
                Path = _publicStorageStatePath
            });
            _authStorageStatePath = _publicStorageStatePath;
        }
        finally
        {
            await context.CloseAsync();
        }
    }

    private async Task CreateAuthenticationStorageStateAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 }
        });
        context.Close += (_, _) => LogFixture("Authentication browser context closed.");

        try
        {
            var page = await context.NewPageAsync();
            AttachBrowserDiagnostics(page);
            LogFixture("Authentication setup started.");

            LogFixture("Navigating to the login page.");
            await GotoAsync(page, "/auth/login", 1280, 800);
            LogFixture($"Login page ready. URL: {page.Url}");
            await EnsureDemoModeAsync(page);

            LogFixture("Filling username field.");
            await page.FillAsync(LoginUsernameSelector, Username);
            LogFixture("Username field filled.");
            LogFixture("Filling password field.");
            await page.FillAsync(LoginPasswordSelector, Password);
            LogFixture("Password field filled.");

            var loginSubmitButton = page.Locator("button[type='submit']");
            await loginSubmitButton.WaitForAsync(new LocatorWaitForOptions
            {
                State = WaitForSelectorState.Visible,
                Timeout = 15000
            });
            LogFixture(
                $"Login submit button ready. Count: {await loginSubmitButton.CountAsync()}, " +
                $"enabled: {await loginSubmitButton.IsEnabledAsync()}.");

            // Blazor performs client-side navigation; do not make the click action wait for navigation.
            LogFixture("Dispatching login submit click.");
            await loginSubmitButton.DispatchEventAsync("click");
            LogFixture("Login submit click dispatched.");

            LogFixture("Waiting for the Blazor route and auth token.");
            await page.WaitForFunctionAsync(
                "() => window.location.pathname === '/' && !!localStorage.getItem('AuthToken')",
                null,
                new PageWaitForFunctionOptions { Timeout = 60000 });

            LogFixture($"Login state reached. URL: {page.Url}");
            await WaitForPageReadyAsync(page, "/");
            await EnsureDemoModeAsync(page);
            LogFixture("Authentication setup completed.");

            _authStorageStatePath = Path.Combine(
                Path.GetTempPath(),
                $"ThaiX-visualtests-auth-{Guid.NewGuid():N}.json");
            await context.StorageStateAsync(new BrowserContextStorageStateOptions
            {
                Path = _authStorageStatePath
            });
        }
        catch (Exception ex)
        {
            LogFixture($"Authentication setup failed with original exception: {ex}");

            string diagnostics;
            try
            {
                diagnostics = await GetAuthenticationDiagnosticsAsync(context.Pages.FirstOrDefault()!, Browser);
            }
            catch (Exception diagnosticException)
            {
                LogFixture($"Authentication diagnostics collection failed: {diagnosticException}");
                diagnostics =
                    $"Diagnostics unavailable: {diagnosticException.GetType().Name}: {diagnosticException.Message}";
            }

            throw new InvalidOperationException(
                $"Visual test authentication setup failed.{Environment.NewLine}{diagnostics}",
                ex);
        }
        finally
        {
            await context.CloseAsync();
        }
    }

    private async Task CreatePublicStorageStateAsync()
    {
        var context = await Browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            ViewportSize = new ViewportSize { Width = 1280, Height = 800 }
        });

        try
        {
            var page = await context.NewPageAsync();
            AttachBrowserDiagnostics(page);
            await GotoAsync(page, "/auth/login", 1280, 800);
            await EnsureDemoModeAsync(page);

            _publicStorageStatePath = Path.Combine(
                Path.GetTempPath(),
                $"ThaiX-visualtests-public-{Guid.NewGuid():N}.json");
            await context.StorageStateAsync(new BrowserContextStorageStateOptions
            {
                Path = _publicStorageStatePath
            });
        }
        finally
        {
            await context.CloseAsync();
        }
    }

    /// <summary>
    /// Opens an isolated browser context + page (safe for parallel workers). Caller must dispose the context.
    /// </summary>
    public async Task<(IBrowserContext Context, IPage Page)> CreatePageAsync(
        bool authenticated, int width, int height, string? locale = null)
    {
        var storagePath = authenticated ? _authStorageStatePath : _publicStorageStatePath;
        if (string.IsNullOrEmpty(storagePath))
        {
            throw new InvalidOperationException("AuthFixture storage state was not initialized.");
        }

        var contextOptions = new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true,
            StorageStatePath = storagePath,
            ViewportSize = new ViewportSize { Width = width, Height = height }
        };

        if (!string.IsNullOrWhiteSpace(locale))
        {
            contextOptions.Locale = locale;
        }

        var context = await Browser.NewContextAsync(contextOptions);
        var page = await context.NewPageAsync();
        AttachBrowserDiagnostics(page);
        LogFixture($"Created {(authenticated ? "authenticated" : "guest")} page. Viewport: {width}x{height}, locale: {locale ?? "default"}.");
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
    public async Task GotoAsync(
        IPage page, string route, int width, int height, int settleMs = 300, int networkIdleTimeoutMs = 30000)
    {
        if (page.ViewportSize is null || page.ViewportSize.Width != width || page.ViewportSize.Height != height)
        {
            await page.SetViewportSizeAsync(width, height);
        }

        var pageErrors = new List<string>();
        var failedRequests = new List<string>();
        page.PageError += (_, error) => pageErrors.Add(error);
        page.RequestFailed += (_, request) =>
            failedRequests.Add($"{request.Method} {request.Url}: {request.Failure}");

        await page.GotoAsync($"{BaseUrl}{route}", new PageGotoOptions
        {
            WaitUntil = WaitUntilState.DOMContentLoaded,
            Timeout = 120000
        });

        try
        {
            await WaitForPageReadyAsync(page, route, networkIdleTimeoutMs);
        }
        catch (TimeoutException ex)
        {
            var diagnostics = await GetStartupDiagnosticsAsync(page, pageErrors, failedRequests);
            throw new TimeoutException($"{ex.Message}{Environment.NewLine}{diagnostics}", ex);
        }

        if (settleMs > 0)
        {
            await page.WaitForTimeoutAsync(settleMs);
        }
    }

    /// <summary>
    /// Boot splash (.loading-progress) must be gone; then wait for route-specific UI.
    /// Full page reloads re-download WASM — this often takes tens of seconds in Docker.
    /// </summary>
    public static async Task WaitForPageReadyAsync(IPage page, string route, int networkIdleTimeoutMs = 30000)
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

        if (networkIdleTimeoutMs <= 0)
        {
            return;
        }

        try
        {
            await page.WaitForLoadStateAsync(
                LoadState.NetworkIdle,
                new PageWaitForLoadStateOptions { Timeout = networkIdleTimeoutMs });
        }
        catch (TimeoutException)
        {
            // Some pages keep websockets/polling; content wait above is enough.
        }
    }

    private static void AttachBrowserDiagnostics(IPage page)
    {
        page.Close += (_, _) => LogFixture("Browser page closed.");
        page.Crash += (_, _) => LogFixture("Browser page crashed.");
        page.Console += (_, message) =>
        {
            if (message.Type == "error" || message.Type == "warning")
            {
                LogFixture($"Browser console {message.Type}: {message.Text}");
            }
        };
        page.PageError += (_, error) => LogFixture($"Browser page error: {error}");
        page.RequestFailed += (_, request) =>
            LogFixture($"Request failed: {request.Method} {GetSafeUrl(request.Url)} ({request.Failure ?? "unknown"})");
        page.Request += (_, request) =>
        {
            if (request.Method is "POST" or "PUT" or "PATCH" or "DELETE")
            {
                LogFixture($"HTTP request: {request.Method} {GetSafeUrl(request.Url)}");
            }
        };
        page.Response += (_, response) =>
        {
            var isLoginResponse = GetSafeUrl(response.Url).Contains(
                "/api/auth/login",
                StringComparison.OrdinalIgnoreCase);
            if (response.Status >= 400 || isLoginResponse)
            {
                LogFixture($"HTTP response: {response.Status} {response.StatusText} {response.Request.Method} {GetSafeUrl(response.Url)}");
            }
        };
    }

    private static string GetSafeUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var uri)
            ? $"{uri.GetLeftPart(UriPartial.Path)}"
            : url.Split('?', 2)[0];
    }

    private static void LogFixture(string message)
    {
        var entry = $"[ThaiX.VisualTests] {DateTimeOffset.Now:O} {message}";
        Console.WriteLine(entry);

        try
        {
            lock (LogGate)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(LogFilePath)!);
                File.AppendAllText(LogFilePath, entry + Environment.NewLine);
            }
        }
        catch (IOException)
        {
            // Diagnostics must never fail a visual test.
        }
        catch (UnauthorizedAccessException)
        {
            // Console output remains available when the log file cannot be written.
        }
    }

    private static async Task<string> GetAuthenticationDiagnosticsAsync(IPage page, IBrowser browser)
    {
        var browserConnected = browser.IsConnected;
        var pageClosed = page.IsClosed;

        if (!browserConnected || pageClosed)
        {
            return string.Join(
                Environment.NewLine,
                "Authentication diagnostics:",
                $"Browser connected: {browserConnected}",
                $"Page closed: {pageClosed}",
                "Browser state evaluation skipped because the Playwright target is unavailable.");
        }

        string browserState;
        try
        {
            browserState = await page.EvaluateAsync<string>(
                """
                () => JSON.stringify({
                    url: location.href.split("?")[0],
                    title: document.title,
                    readyState: document.readyState,
                    pathname: location.pathname,
                    loadingVisible: !!document.querySelector("svg.loading-progress"),
                    errorUiVisible: !!document.querySelector("#blazor-error-ui") && getComputedStyle(document.querySelector("#blazor-error-ui")).display !== "none",
                    authTokenPresent: !!localStorage.getItem("AuthToken"),
                    submitButtons: Array.from(document.querySelectorAll("button[type='submit']")).map(button => ({
                        disabled: button.disabled,
                        visible: !!(button.offsetWidth || button.offsetHeight || button.getClientRects().length)
                    })),
                    passwordInputCount: document.querySelectorAll("input[type='password']").length,
                    usernameInputCount: document.querySelectorAll("input[name='username'], input[name='Username'], input.rz-textbox").length
                })
                """);
        }
        catch (Exception diagnosticException)
        {
            browserState = $"Browser state unavailable ({diagnosticException.GetType().Name}): {diagnosticException.Message}";
        }

        string pageUrl;
        try
        {
            pageUrl = page.IsClosed ? "<closed>" : page.Url;
        }
        catch (Exception urlException)
        {
            pageUrl = $"<unavailable: {urlException.GetType().Name}>";
        }

        return string.Join(
            Environment.NewLine,
            "Authentication diagnostics:",
            $"Browser connected: {browser.IsConnected}",
            $"Page closed: {page.IsClosed}",
            $"URL: {pageUrl}",
            $"Browser state: {browserState}");
    }

    private static async Task<string> GetStartupDiagnosticsAsync(
        IPage page,
        IReadOnlyCollection<string> pageErrors,
        IReadOnlyCollection<string> failedRequests)
    {
        var browserState = await page.EvaluateAsync<string>(
            """
            () => JSON.stringify({
                title: document.title,
                readyState: document.readyState,
                loadingVisible: !!document.querySelector("svg.loading-progress"),
                errorUi: document.querySelector("#blazor-error-ui")?.textContent?.trim() ?? null,
                scripts: Array.from(document.scripts).map(script => ({
                    src: script.src,
                    loaded: script.readyState ?? "unknown"
                }))
            })
            """);

        return string.Join(
            Environment.NewLine,
            "Browser startup diagnostics:",
            $"State: {browserState}",
            $"Page errors: {(pageErrors.Count == 0 ? "none" : string.Join(" | ", pageErrors))}",
            $"Failed requests: {(failedRequests.Count == 0 ? "none" : string.Join(" | ", failedRequests))}");
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
        var browser = Browser;
        Browser = null!;
        if (browser is not null)
        {
            try
            {
                await browser.CloseAsync();
            }
            catch (PlaywrightException ex)
            {
                LogFixture($"Browser close during fixture cleanup failed: {ex.Message}");
            }
        }

        var playwright = Playwright;
        Playwright = null!;
        playwright?.Dispose();

        try
        {
            await _appHost.DisposeAsync();
        }
        finally
        {
            TryDelete(_authStorageStatePath);
            TryDelete(_publicStorageStatePath);
        }
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
