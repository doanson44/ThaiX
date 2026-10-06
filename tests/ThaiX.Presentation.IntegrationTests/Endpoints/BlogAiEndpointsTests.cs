using System.Net;
using System.Net.Http.Json;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class BlogAiEndpointsTests : IntegrationTestBase
{
    public BlogAiEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GenerateTitle_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.PostAsJsonAsync("/api/blog/ai/generate-title", new { ContentHtml = "content" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GenerateTitle_WithoutBlogWritePermission_ShouldReturnForbidden()
    {
        var email = $"blogai-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.BlogRead });
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.PostAsJsonAsync("/api/blog/ai/generate-title", new { ContentHtml = "content" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GenerateTitle_WithValidRequest_ShouldReturnEchoedText()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/blog/ai/generate-title", new { ContentHtml = "MarketsAreVolatileToday" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<BlogAiTextResponseDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.Text.Should().Contain("MarketsAreVolatileToday");
    }

    [Fact]
    public async Task GenerateSummary_WithValidRequest_ShouldReturnEchoedText()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/blog/ai/generate-summary", new { Title = "MyPostTitle", ContentHtml = "content" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<BlogAiTextResponseDto>(response);
        envelope.Data!.Text.Should().Contain("MyPostTitle");
    }

    [Fact]
    public async Task Rewrite_WithEmptyText_ShouldReturnEmptyTextWithoutCallingAi()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/blog/ai/rewrite", new { Text = "", Tone = "Professional" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<BlogAiTextResponseDto>(response);
        envelope.Data!.Text.Should().BeEmpty();
    }

    [Fact]
    public async Task Rewrite_WithValidText_ShouldReturnEchoedText()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/blog/ai/rewrite", new { Text = "PleaseRewriteThis", Tone = "Casual" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<BlogAiTextResponseDto>(response);
        envelope.Data!.Text.Should().Contain("PleaseRewriteThis");
    }

    [Fact]
    public async Task ImproveGrammar_WithValidText_ShouldReturnEchoedText()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/blog/ai/improve-grammar", new { Text = "ThisTextHasIssue" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<BlogAiTextResponseDto>(response);
        envelope.Data!.Text.Should().Contain("ThisTextHasIssue");
    }

    [Fact]
    public async Task GenerateTags_WithValidRequest_ShouldReturnEchoedText()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/blog/ai/generate-tags", new { Title = "TagTestTitle", ContentHtml = "content" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<BlogAiTextResponseDto>(response);
        envelope.Data!.Text.Should().Contain("TagTestTitle");
    }

    [Fact]
    public async Task GenerateSeo_WithValidRequest_ShouldReturnOkEnvelope()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/blog/ai/generate-seo", new { Title = "SeoTestTitle", Summary = "SeoTestSummary" });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<GenerateSeoResponseDto>(response);
        envelope.Success.Should().BeTrue();
    }

    [Fact]
    public async Task Review_WithValidRequest_ShouldReturnVerdictAndReport()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/blog/ai/review", new
        {
            Title = "ReviewTestTitle",
            Summary = "Summary text",
            ContentHtml = "<h1>Content</h1><p>Body text.</p>",
            MetaTitle = (string?)null,
            MetaDescription = (string?)null,
            CategoryName = (string?)null
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<ReviewPostResponseDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.Verdict.Should().BeOneOf("READY TO PUBLISH", "NEEDS WORK", "REVIEW");
        envelope.Data.Report.Should().Contain("Summary text");
    }

    private sealed record BlogAiTextResponseDto
    {
        public string Text { get; init; } = string.Empty;
    }

    private sealed record GenerateSeoResponseDto
    {
        public string? MetaTitle { get; init; }
        public string? MetaDescription { get; init; }
        public string? Keywords { get; init; }
    }

    private sealed record ReviewPostResponseDto
    {
        public string Verdict { get; init; } = string.Empty;
        public string Report { get; init; } = string.Empty;
    }
}
