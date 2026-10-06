using System.Net;
using System.Net.Http.Json;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class ResumeAiEndpointsTests : IntegrationTestBase
{
    public ResumeAiEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    private async Task AuthenticateWithResumeWriteAsync()
    {
        var email = $"resume-ai-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.ResumeWrite });
        await AuthenticateAsync(email, "Test@Pass123");
    }

    #region POST /api/resumes/ai/polish-text

    [Fact]
    public async Task PolishText_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.PostAsJsonAsync("/api/resumes/ai/polish-text", new { Text = "built stuff" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task PolishText_WithoutResumeWritePermission_ShouldReturnForbidden()
    {
        var email = $"resume-ai-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.PostAsJsonAsync("/api/resumes/ai/polish-text", new { Text = "built stuff" });

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task PolishText_WithValidText_ShouldReturnRewrittenText()
    {
        await AuthenticateWithResumeWriteAsync();

        var response = await Client.PostAsJsonAsync("/api/resumes/ai/polish-text", new
        {
            Text = "built stuff",
            FieldContext = "job description for Engineer at Acme"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<ResumeAiTextResponseDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.Text.Should().StartWith("Generated:");
        envelope.Data.Text.Should().Contain("built stuff");
    }

    [Fact]
    public async Task PolishText_WithBlankText_ShouldReturnBlankTextWithoutCallingAi()
    {
        await AuthenticateWithResumeWriteAsync();

        var response = await Client.PostAsJsonAsync("/api/resumes/ai/polish-text", new { Text = "   " });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<ResumeAiTextResponseDto>(response);
        envelope.Data!.Text.Should().BeEmpty();
    }

    #endregion

    #region POST /api/resumes/ai/generate-summary

    [Fact]
    public async Task GenerateSummary_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.PostAsJsonAsync("/api/resumes/ai/generate-summary", new { FullName = "Jane" });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GenerateSummary_WithValidData_ShouldReturnSummaryText()
    {
        await AuthenticateWithResumeWriteAsync();

        var response = await Client.PostAsJsonAsync("/api/resumes/ai/generate-summary", new
        {
            FullName = "Jane Doe",
            Headline = "Software Engineer",
            Skills = new[] { "C#", "SQL" },
            Experience = new[] { new { Company = "Acme", Position = "Engineer", StartDate = "2022-01", EndDate = (string?)null, Description = "Built things." } },
            Projects = Array.Empty<object>(),
            Education = Array.Empty<object>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<ResumeAiTextResponseDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.Text.Should().Contain("Jane Doe");
        envelope.Data.Text.Should().Contain("Acme");
    }

    #endregion

    #region POST /api/resumes/ai/optimize-for-job

    [Fact]
    public async Task OptimizeForJob_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.PostAsJsonAsync("/api/resumes/ai/optimize-for-job", new
        {
            JobDescription = "Looking for a backend engineer.",
            Content = new { }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task OptimizeForJob_WithValidData_ShouldReturnReport()
    {
        await AuthenticateWithResumeWriteAsync();

        var response = await Client.PostAsJsonAsync("/api/resumes/ai/optimize-for-job", new
        {
            JobDescription = "Looking for a backend engineer skilled in C# and SQL.",
            Content = new
            {
                Summary = "",
                Skills = new[] { "C#" },
                Experience = Array.Empty<object>(),
                Projects = Array.Empty<object>(),
                Education = Array.Empty<object>(),
                Links = Array.Empty<object>()
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<ResumeAiReportResponseDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.Report.Should().Contain("backend engineer");
    }

    #endregion

    #region POST /api/resumes/ai/check-consistency

    [Fact]
    public async Task CheckConsistency_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.PostAsJsonAsync("/api/resumes/ai/check-consistency", new { Content = new { } });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CheckConsistency_WithValidData_ShouldReturnReport()
    {
        await AuthenticateWithResumeWriteAsync();

        var response = await Client.PostAsJsonAsync("/api/resumes/ai/check-consistency", new
        {
            Content = new
            {
                Summary = "I am building things.",
                Skills = Array.Empty<string>(),
                Experience = Array.Empty<object>(),
                Projects = Array.Empty<object>(),
                Education = Array.Empty<object>(),
                Links = Array.Empty<object>()
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<ResumeAiReportResponseDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.Report.Should().StartWith("Generated:");
    }

    #endregion

    private sealed record ResumeAiTextResponseDto
    {
        public string Text { get; init; } = string.Empty;
    }

    private sealed record ResumeAiReportResponseDto
    {
        public string Report { get; init; } = string.Empty;
    }
}
