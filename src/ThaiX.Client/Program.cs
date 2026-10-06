using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.JSInterop;
using Radzen;
using System.Globalization;
using ThaiX.Client;
using ThaiX.Client.Constants;
using ThaiX.Client.Services.Ai;
using ThaiX.Client.Services.AssetPositions;
using ThaiX.Client.Services.Auth;
using ThaiX.Client.Services.Caching;
using ThaiX.Client.Services.Blog;
using ThaiX.Client.Services.Breadcrumb;
using ThaiX.Client.Services.Contacts;
using ThaiX.Client.Services.CredentialAccounts;
using ThaiX.Client.Services.ExpenseTracker;
using ThaiX.Client.Services.ExternalData;
using ThaiX.Client.Services.Files;
using ThaiX.Client.Services.JsonBins;
using ThaiX.Client.Services.Tools;
using ThaiX.Client.Services.Http;
using ThaiX.Client.Services.Http.Demo;
using ThaiX.Client.Services.Lottery;
using ThaiX.Client.Services.MarketData;
using ThaiX.Client.Services.MarketScanner;
using ThaiX.Client.Services.MasterData;
using ThaiX.Client.Services.Notes;
using ThaiX.Client.Services.Notifications;
using ThaiX.Client.Services.Portfolios;
using ThaiX.Client.Services.PriceAlerts;
using ThaiX.Client.Services.Resumes;
using ThaiX.Client.Services.Slack;
using ThaiX.Client.Services.Ui;
using ThaiX.Client.Services.Users;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Keep .resx beside Localization/SharedResource.cs. ResourcesPath breaks the embed name match.
builder.Services.AddLocalization();
builder.Services.AddAuthorizationCore(options =>
{
    foreach (var permission in PermissionNames.GetAll())
    {
        options.AddPolicy(permission, policy => policy.RequireClaim("scope", permission));
    }
});
builder.Services.AddScoped<AuthTokenStore>();
builder.Services.AddScoped<JwtAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp =>
    sp.GetRequiredService<JwtAuthenticationStateProvider>());

var configuration = builder.Configuration;
var apiBaseUrl = configuration["Api:BaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    apiBaseUrl = builder.HostEnvironment.BaseAddress;
}

var forceDemo = configuration.GetValue("Api:ForceDemo", false);
var healthTimeoutSeconds = configuration.GetValue("Api:HealthTimeoutSeconds", 3);
string? probeFailure = null;
var apiOnline = false;

if (forceDemo)
{
    probeFailure = "Api:ForceDemo is true.";
}
else
{
    (apiOnline, probeFailure) = await ApiHealthProbe.ProbeAsync(apiBaseUrl, healthTimeoutSeconds);
}

var isDemoMode = forceDemo || !apiOnline;
var connectivity = new ApiConnectivityState
{
    IsDemoMode = isDemoMode,
    ApiBaseUrl = apiBaseUrl,
    ProbeFailureReason = isDemoMode ? probeFailure : null
};
builder.Services.AddSingleton(connectivity);

if (isDemoMode)
{
    Console.WriteLine($"ThaiX Client running in DEMO mode (API unreachable). Reason: {probeFailure}");
    builder.Services.AddSingleton<DemoSessionStore>();
    builder.Services.AddSingleton<DemoResponseFactory>();
}

// Register a scoped HttpClient that resolves the auth handler from the SAME scope.
builder.Services.AddScoped(sp =>
{
    var tokenStore = sp.GetRequiredService<AuthTokenStore>();
    var state = sp.GetRequiredService<ApiConnectivityState>();

    HttpMessageHandler inner = state.IsDemoMode
        ? new DemoApiMessageHandler(sp.GetRequiredService<DemoResponseFactory>())
        : new HttpClientHandler();

    var handler = new ApiAuthorizationMessageHandler(tokenStore)
    {
        InnerHandler = inner
    };

    return new HttpClient(handler) { BaseAddress = new Uri(state.ApiBaseUrl) };
});

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBreadcrumbService, BreadcrumbService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.Configure<ClientCacheOptions>(configuration.GetSection(ClientCacheOptions.SectionName));
builder.Services.AddSingleton<IClientCacheService, ClientCacheService>();
builder.Services.AddScoped<IMasterDataService, MasterDataService>();
builder.Services.AddScoped<IContactService, ContactService>();
builder.Services.AddScoped<IAiService, AiService>();
builder.Services.AddScoped<ISlackService, SlackService>();
builder.Services.AddScoped<IBotCommandTesterService, BotCommandTesterService>();
builder.Services.AddScoped<INotificationService, NotificationApiService>();
builder.Services.AddScoped<INotificationScheduleService, NotificationScheduleService>();
builder.Services.AddScoped<IFileDownloadService, FileDownloadService>();
builder.Services.AddScoped<IFileService, FileService>();
builder.Services.AddScoped<IExternalDataService, ExternalDataService>();
builder.Services.AddScoped<IViewportService, ViewportService>();
builder.Services.AddScoped<IThemeService, AppThemeService>();
builder.Services.AddScoped<IMarketScannerService, MarketScannerService>();
builder.Services.AddScoped<IPriceAlertService, PriceAlertService>();
builder.Services.AddScoped<IExpenseTrackerService, ExpenseTrackerService>();
builder.Services.AddScoped<ICredentialAccountService, CredentialAccountService>();
builder.Services.AddScoped<IPortfolioService, PortfolioService>();
builder.Services.AddScoped<IAssetPositionService, AssetPositionService>();
builder.Services.AddScoped<INoteService, NoteService>();
builder.Services.AddScoped<ILotteryService, LotteryService>();
builder.Services.AddScoped<IResumeService, ResumeService>();
builder.Services.AddScoped<IBlogService, BlogService>();
builder.Services.AddScoped<ThaiX.Client.Services.ApiClients.IApiClientService, ThaiX.Client.Services.ApiClients.ApiClientService>();
builder.Services.AddScoped<IJsonBinService, JsonBinService>();
builder.Services.AddScoped<IToolStateStore, ToolStateStore>();
builder.Services.AddTransient<IMexcContractTickerSocketService, MexcContractTickerSocketService>();
builder.Services.AddTransient<IMexcSpotTickerSocketService, MexcSpotTickerSocketService>();
builder.Services.AddScoped<MexcSocketPriceMoveAlertStore>();

builder.Services.AddRadzenComponents();

var host = builder.Build();
await ConfigureCultureAsync(host);

if (connectivity.IsDemoMode)
{
    Console.WriteLine("Demo mode active — all REST API calls use in-memory dummy data. Login with any credentials as admin.");
}

await host.RunAsync();

static async Task ConfigureCultureAsync(WebAssemblyHost host)
{
    const string defaultCulture = "en-US";

    var jsRuntime = host.Services.GetRequiredService<IJSRuntime>();
    var storedCulture = await jsRuntime.InvokeAsync<string>("blazorCulture.get");
    var cultureName = string.IsNullOrWhiteSpace(storedCulture) ? defaultCulture : storedCulture;

    var culture = new CultureInfo(cultureName);
    CultureInfo.DefaultThreadCurrentCulture = culture;
    CultureInfo.DefaultThreadCurrentUICulture = culture;
}
