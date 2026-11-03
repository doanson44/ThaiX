using System.Net;
using System.Net.Http.Headers;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class AgentDiscoveryEndpointsTests : IntegrationTestBase
{
    public AgentDiscoveryEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetRobotsTxt_ShouldExposeCrawlerPolicy()
    {
        // Act
        var response = await Client.GetAsync("/robots.txt");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        content.Should().Contain("User-agent: OAI-SearchBot");
        content.Should().Contain("User-agent: Claude-SearchBot");
        content.Should().Contain("User-agent: PerplexityBot");
        content.Should().Contain("Content-Signal: ai-train=no, search=yes, ai-input=no");
    }

    [Fact]
    public async Task GetApiCatalog_ShouldReturnLinksetJson()
    {
        // Act
        var response = await Client.GetAsync("/.well-known/api-catalog");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/linkset+json");
        content.Should().Contain("\"service-doc\"");
        content.Should().Contain("\"status\"");
    }

    [Fact]
    public async Task GetHomepage_WithMarkdownAccept_ShouldReturnMarkdownAndDiscoveryLinks()
    {
        // Arrange
        using var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("text/markdown"));

        // Act
        var response = await Client.SendAsync(request);
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/markdown");
        response.Headers.Contains("Link").Should().BeTrue();
        content.Should().Contain("# ThaiX");
        content.Should().Contain("/api.md");
    }
}
