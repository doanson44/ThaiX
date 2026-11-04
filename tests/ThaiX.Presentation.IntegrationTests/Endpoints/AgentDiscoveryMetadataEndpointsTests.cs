using System.Net;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text.Json;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class AgentDiscoveryMetadataEndpointsTests : IntegrationTestBase
{
    public AgentDiscoveryMetadataEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetAgentSkillsIndex_ShouldReturnValidDigestAndCors()
    {
        // Act
        var response = await Client.GetAsync("/.well-known/agent-skills/index.json");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        response.Headers.GetValues("Access-Control-Allow-Origin").Should().Contain("*");

        using var document = JsonDocument.Parse(content);
        var skill = document.RootElement.GetProperty("skills")[0];
        var digest = skill.GetProperty("digest").GetString();
        digest.Should().StartWith("sha256:");
        digest.Should().HaveLength(71);

        var artifactResponse = await Client.GetAsync(skill.GetProperty("url").GetString());
        artifactResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        artifactResponse.Content.Headers.ContentType!.MediaType.Should().Be("text/markdown");
        var artifact = await artifactResponse.Content.ReadAsByteArrayAsync();
        var actualDigest = Convert.ToHexString(SHA256.HashData(artifact)).ToLowerInvariant();
        digest.Should().Be("sha256:" + actualDigest);
    }

    [Fact]
    public async Task GetAgentCatalog_ShouldReturnArdManifestWithRepresentativeQueries()
    {
        // Act
        var response = await Client.GetAsync("/.well-known/ai-catalog.json");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("application/json");
        response.Headers.GetValues("Access-Control-Allow-Origin").Should().Contain("*");

        using var document = JsonDocument.Parse(content);
        var root = document.RootElement;
        root.GetProperty("specVersion").GetString().Should().Be("1.0");
        root.GetProperty("host").GetProperty("displayName").GetString().Should().Be("ThaiX");

        var entries = root.GetProperty("entries");
        entries.GetArrayLength().Should().BeGreaterThan(0);
        foreach (var entry in entries.EnumerateArray())
        {
            entry.GetProperty("identifier").GetString().Should().StartWith("urn:air:");
            entry.GetProperty("representativeQueries").GetArrayLength().Should().BeInRange(2, 5);
            (entry.TryGetProperty("url", out _) ^ entry.TryGetProperty("data", out _)).Should().BeTrue();
        }
    }

    [Fact]
    public async Task GetAuthMarkdown_ShouldUseExpectedHeadingAndDescribeRealAuthentication()
    {
        // Act
        var response = await Client.GetAsync("/auth.md");
        var content = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/markdown");
        content.Should().StartWith("# Auth.md");
        content.Should().Contain("/api/token/client-credentials");
        content.Should().Contain("does not operate an OAuth 2.0 authorization server");
    }

    [Fact]
    public async Task GetHomepage_WithMarkdownAccept_ShouldAdvertiseAgentDiscoveryResources()
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
        response.Headers.GetValues("Link").Single().Should().Contain("/.well-known/ai-catalog.json");
        content.Should().Contain("/.well-known/agent-skills/index.json");
    }
}
