using System.Net.Sockets;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using DotNet.Testcontainers.Images;

namespace ThaiX.Client.VisualTests;

/// <summary>
/// Builds a Docker image for <c>ThaiX.Client</c> (SDK + DevServer, VisualTests / ForceDemo),
/// starts the container, then exposes <see cref="BaseUrl"/> for Playwright.
/// Set <c>APP_BASE_URL</c> to attach to an already-running app instead (skips Docker).
/// </summary>
internal sealed class ClientAppHost : IAsyncDisposable
{
    private const int AppPort = 8080;
    private const string ImageRepository = "thaix-client-visualtests";

    private IFutureDockerImage? _image;
    private IContainer? _container;

    public string BaseUrl { get; private set; } = string.Empty;

    public bool StartedByFixture { get; private set; }

    public async Task StartAsync(CancellationToken cancellationToken = default)
    {
        var overrideUrl = Environment.GetEnvironmentVariable("APP_BASE_URL");
        if (!string.IsNullOrWhiteSpace(overrideUrl))
        {
            BaseUrl = overrideUrl.TrimEnd('/');
            if (!await IsReachableAsync(BaseUrl, cancellationToken))
            {
                throw new InvalidOperationException(
                    $"APP_BASE_URL is set to {BaseUrl} but the app is not reachable. " +
                    "Start the client container, or unset APP_BASE_URL so the fixture builds and starts Docker.");
            }

            return;
        }

        var repoRoot = ResolveRepoRoot();
        // Testcontainers 4.0: Dockerfile directory = build context; Dockerfile path is relative to it.
        // Docker Engine expects forward slashes even on Windows.
        _image = new ImageFromDockerfileBuilder()
            .WithDockerfileDirectory(repoRoot)
            .WithDockerfile("tests/ThaiX.Client.VisualTests/Dockerfile")
            .WithName($"{ImageRepository}:{DateTime.UtcNow:yyyyMMddHHmmss}")
            .WithCleanUp(true)
            .Build();

        await _image.CreateAsync(cancellationToken);

        _container = new ContainerBuilder()
            .WithImage(_image)
            .WithName($"thaix-client-visualtests-{Guid.NewGuid():N}"[..48])
            .WithPortBinding(AppPort, true)
            .WithEnvironment("ASPNETCORE_ENVIRONMENT", "VisualTests")
            .WithEnvironment("ASPNETCORE_URLS", $"http://0.0.0.0:{AppPort}")
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilHttpRequestIsSucceeded(r => r
                        .ForPort(AppPort)
                        .ForPath("/")))
            .WithCleanUp(true)
            .Build();

        await _container.StartAsync(cancellationToken);
        StartedByFixture = true;

        var hostPort = _container.GetMappedPublicPort(AppPort);
        BaseUrl = $"http://127.0.0.1:{hostPort}";

        if (!await IsReachableAsync(BaseUrl, cancellationToken))
        {
            var logs = await _container.GetLogsAsync(timestampsEnabled: false);
            throw new InvalidOperationException(
                $"Docker client container started but {BaseUrl} is not reachable.\n{logs.Stdout}\n{logs.Stderr}");
        }
    }

    private static string ResolveRepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null)
        {
            if (File.Exists(Path.Combine(dir.FullName, "ThaiX.sln")) &&
                File.Exists(Path.Combine(dir.FullName, "src", "ThaiX.Client", "ThaiX.Client.csproj")))
            {
                return dir.FullName;
            }

            dir = dir.Parent;
        }

        throw new InvalidOperationException(
            "Could not locate repo root (ThaiX.sln + src/ThaiX.Client) from the test output directory.");
    }

    private static async Task<bool> IsReachableAsync(string baseUrl, CancellationToken cancellationToken = default)
    {
        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri))
        {
            return false;
        }

        try
        {
            using var client = new HttpClient(new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback = static (_, _, _, _) => true
            })
            {
                Timeout = TimeSpan.FromSeconds(5)
            };

            using var response = await client.GetAsync(uri, cancellationToken);
            return response.IsSuccessStatusCode || (int)response.StatusCode is >= 300 and < 500;
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or SocketException)
        {
            return false;
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_container is not null && StartedByFixture)
        {
            await _container.DisposeAsync();
            _container = null;
        }

        if (_image is not null)
        {
            await _image.DisposeAsync();
            _image = null;
        }
    }
}
