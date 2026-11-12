using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using ThaiX.Client.Models.Auth;
using ThaiX.Client.Models.Games;
using ThaiX.Client.Services.Http.Demo;

namespace ThaiX.Client.UnitTests.Services.Http.Demo;

public sealed class DemoEndpointCoverageTests
{
    [Fact]
    public async Task PublicHtmlList_ShouldReturnRawPageArray()
    {
        // Arrange
        var factory = new DemoResponseFactory(new DemoSessionStore());
        using var request = CreateRequest(HttpMethod.Get, "api/public-html");

        // Act
        using var response = await factory.CreateAsync(request);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal(JsonValueKind.Array, document.RootElement.ValueKind);
        Assert.True(document.RootElement.GetArrayLength() > 0);
        Assert.True(document.RootElement[0].TryGetProperty("slug", out _));
    }

    [Fact]
    public async Task GameEndpoints_ShouldReturnTypedResponseEnvelopes()
    {
        // Arrange
        var factory = new DemoResponseFactory(new DemoSessionStore());
        using var leaderboardRequest = CreateRequest(HttpMethod.Get, "api/games/leaderboards/2048?mode=classic");
        using var sessionRequest = CreateRequest(HttpMethod.Post, "api/games/guest-session");
        using var scoreRequest = CreateRequest(
            HttpMethod.Post,
            "api/games/scores",
            JsonContent.Create(new SubmitGameScoreModel("2048", Guid.NewGuid(), 128, 60, "classic", null, null, "Demo Player")));

        // Act
        using var leaderboardResponse = await factory.CreateAsync(leaderboardRequest);
        using var leaderboard = await JsonDocument.ParseAsync(await leaderboardResponse.Content.ReadAsStreamAsync());
        using var sessionResponse = await factory.CreateAsync(sessionRequest);
        using var session = await JsonDocument.ParseAsync(await sessionResponse.Content.ReadAsStreamAsync());
        using var scoreResponse = await factory.CreateAsync(scoreRequest);
        using var score = await JsonDocument.ParseAsync(await scoreResponse.Content.ReadAsStreamAsync());

        // Assert
        Assert.Equal("2048", leaderboard.RootElement.GetProperty("data").GetProperty("gameId").GetString());
        Assert.True(leaderboard.RootElement.GetProperty("data").GetProperty("entries").GetArrayLength() > 0);
        Assert.True(session.RootElement.GetProperty("data").TryGetProperty("token", out _));
        Assert.Equal(1, score.RootElement.GetProperty("data").GetProperty("rank").GetInt32());
    }

    [Fact]
    public async Task EmailOtpEndpoints_ShouldReturnChallengeAndLoginData()
    {
        // Arrange
        var factory = new DemoResponseFactory(new DemoSessionStore());
        using var requestOtp = CreateRequest(
            HttpMethod.Post,
            "api/auth/email-otp/request",
            JsonContent.Create(new RequestEmailOtpRequest { Email = "demo@local" }));

        // Act
        using var challengeResponse = await factory.CreateAsync(requestOtp);
        using var challenge = await JsonDocument.ParseAsync(await challengeResponse.Content.ReadAsStreamAsync());
        var challengeId = challenge.RootElement.GetProperty("data").GetProperty("challengeId").GetGuid();
        using var verifyRequest = CreateRequest(
            HttpMethod.Post,
            "api/auth/email-otp/verify",
            JsonContent.Create(new VerifyEmailOtpRequest { ChallengeId = challengeId, Code = "123456" }));
        using var loginResponse = await factory.CreateAsync(verifyRequest);
        using var login = await JsonDocument.ParseAsync(await loginResponse.Content.ReadAsStreamAsync());

        // Assert
        Assert.NotEqual(Guid.Empty, challengeId);
        Assert.True(challenge.RootElement.GetProperty("data").GetProperty("codeLength").GetInt32() > 0);
        Assert.False(string.IsNullOrWhiteSpace(login.RootElement.GetProperty("data").GetProperty("token").GetString()));
    }

    [Fact]
    public async Task PublicDonateConfig_ShouldBeDisabledInDemoMode()
    {
        // Arrange
        var factory = new DemoResponseFactory(new DemoSessionStore());
        using var request = CreateRequest(HttpMethod.Get, "api/public/donate-config");

        // Act
        using var response = await factory.CreateAsync(request);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        // Assert
        Assert.True(document.RootElement.GetProperty("success").GetBoolean());
        Assert.False(document.RootElement.GetProperty("data").GetProperty("enabled").GetBoolean());
    }

    [Fact]
    public async Task CmsxProxyTester_ShouldNotSendProxyRequestInDemoMode()
    {
        // Arrange
        var factory = new DemoResponseFactory(new DemoSessionStore());
        using var request = CreateRequest(HttpMethod.Post, "api/developer-tools/cmsx-proxy/test", JsonContent.Create(new { url = "https://example.com", method = "GET" }));

        // Act
        using var response = await factory.CreateAsync(request);
        using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("not sent", document.RootElement.GetProperty("data").GetProperty("body").GetString(), StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SystemLogEndpoints_ShouldReturnRawListPreviewAndDownload()
    {
        // Arrange
        var factory = new DemoResponseFactory(new DemoSessionStore());
        using var listRequest = CreateRequest(HttpMethod.Get, "api/system-logs");

        // Act
        using var listResponse = await factory.CreateAsync(listRequest);
        using var list = await JsonDocument.ParseAsync(await listResponse.Content.ReadAsStreamAsync());
        var fileName = list.RootElement[0].GetProperty("fileName").GetString();
        using var previewRequest = CreateRequest(HttpMethod.Get, $"api/system-logs/{fileName}");
        using var previewResponse = await factory.CreateAsync(previewRequest);
        using var preview = await JsonDocument.ParseAsync(await previewResponse.Content.ReadAsStreamAsync());
        using var downloadRequest = CreateRequest(HttpMethod.Get, $"api/system-logs/{fileName}/download");
        using var downloadResponse = await factory.CreateAsync(downloadRequest);
        var downloadContent = await downloadResponse.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(JsonValueKind.Array, list.RootElement.ValueKind);
        Assert.Equal(fileName, preview.RootElement.GetProperty("fileName").GetString());
        Assert.Contains("demo mode", downloadContent, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task UnknownApiEndpoint_ShouldReturnNotImplemented()
    {
        // Arrange
        var factory = new DemoResponseFactory(new DemoSessionStore());
        using var request = CreateRequest(HttpMethod.Get, "api/unknown-demo-endpoint");

        // Act
        using var response = await factory.CreateAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.NotImplemented, response.StatusCode);
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string path, HttpContent? content = null) =>
        new(method, new Uri($"https://localhost/{path}")) { Content = content };
}
