using System.Net;
using System.Net.Http.Json;
using ThaiX.Application.Features.Resumes;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class ResumeEndpointsTests : IntegrationTestBase
{
    public ResumeEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    private static object BuildUpsertRequest(string slug, bool isPublished = true, string fullName = "Jane Doe") => new
    {
        Slug = slug,
        FullName = fullName,
        Headline = "Software Engineer",
        MetaDescription = "Experienced backend engineer.",
        IsPublished = isPublished,
        Content = new
        {
            Summary = "Building reliable systems for five years.",
            Skills = new[] { "C#", "SQL", "Azure" },
            Experience = new[]
            {
                new { Company = "Acme", Position = "Engineer", StartDate = "2022-01", EndDate = (string?)null, Description = "Built things." }
            },
            Projects = Array.Empty<object>(),
            Education = Array.Empty<object>(),
            Links = new[] { new { Label = "GitHub", Url = "https://github.com/janedoe" } }
        }
    };

    #region GET /api/resumes/mine

    [Fact]
    public async Task GetMine_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.GetAsync("/api/resumes/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetMine_WithoutResumeReadPermission_ShouldReturnForbidden()
    {
        var email = $"resume-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.GetAsync("/api/resumes/mine");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetMine_WhenNoResumeCreatedYet_ShouldReturnNotFound()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await Client.GetAsync("/api/resumes/mine");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetMine_AfterSaving_ShouldReturnSavedResume()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"jane-{Guid.NewGuid():N}";
        await Client.PutAsJsonAsync("/api/resumes/mine", BuildUpsertRequest(slug));

        var response = await Client.GetAsync("/api/resumes/mine");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<ResumeDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.Slug.Should().Be(slug);
        envelope.Data.FullName.Should().Be("Jane Doe");
        envelope.Data.Content.Skills.Should().Contain("C#");
        envelope.Data.Content.Experience.Should().ContainSingle(e => e.Company == "Acme");
    }

    #endregion

    #region PUT /api/resumes/mine

    [Fact]
    public async Task SaveMine_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.PutAsJsonAsync("/api/resumes/mine", BuildUpsertRequest("no-auth"));

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task SaveMine_WithoutResumeWritePermission_ShouldReturnForbidden()
    {
        var email = $"resume-write-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.PutAsJsonAsync("/api/resumes/mine", BuildUpsertRequest("no-perm"));

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task SaveMine_WithValidData_ShouldReturnOkAndPersist()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"valid-{Guid.NewGuid():N}";

        var response = await Client.PutAsJsonAsync("/api/resumes/mine", BuildUpsertRequest(slug));

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<Guid>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task SaveMine_CalledTwice_ShouldUpdateSameResumeNotCreateSecond()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"upsert-{Guid.NewGuid():N}";

        var first = await Client.PutAsJsonAsync("/api/resumes/mine", BuildUpsertRequest(slug, fullName: "First Name"));
        var firstId = (await ReadResponseAsync<Guid>(first)).Data;

        var second = await Client.PutAsJsonAsync("/api/resumes/mine", BuildUpsertRequest(slug, fullName: "Second Name"));
        var secondId = (await ReadResponseAsync<Guid>(second)).Data;

        secondId.Should().Be(firstId);

        var mine = await ReadResponseAsync<ResumeDto>(await Client.GetAsync("/api/resumes/mine"));
        mine.Data!.FullName.Should().Be("Second Name");
    }

    [Fact]
    public async Task SaveMine_WithEmptySlug_ShouldReturnBadRequest()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await Client.PutAsJsonAsync("/api/resumes/mine", BuildUpsertRequest(""));

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SaveMine_WithSlugAlreadyUsedByAnotherUser_ShouldReturnConflict()
    {
        var slug = $"taken-{Guid.NewGuid():N}";

        await CreateAndAuthenticateAdminAsync();
        await Client.PutAsJsonAsync("/api/resumes/mine", BuildUpsertRequest(slug));

        await CreateAndAuthenticateAdminAsync();
        var response = await Client.PutAsJsonAsync("/api/resumes/mine", BuildUpsertRequest(slug));

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    #endregion

    #region GET /api/resumes/public/{slug}

    [Fact]
    public async Task GetPublicBySlug_WithoutAuthentication_ShouldReturnOkWhenPublished()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"public-{Guid.NewGuid():N}";
        await Client.PutAsJsonAsync("/api/resumes/mine", BuildUpsertRequest(slug, isPublished: true));

        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.GetAsync($"/api/resumes/public/{slug}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<ResumeDto>(response);
        envelope.Data!.Slug.Should().Be(slug);
    }

    [Fact]
    public async Task GetPublicBySlug_WhenUnpublished_ShouldReturnNotFound()
    {
        await CreateAndAuthenticateAdminAsync();
        var slug = $"unpublished-{Guid.NewGuid():N}";
        await Client.PutAsJsonAsync("/api/resumes/mine", BuildUpsertRequest(slug, isPublished: false));

        Client.DefaultRequestHeaders.Authorization = null;
        var response = await Client.GetAsync($"/api/resumes/public/{slug}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetPublicBySlug_WhenSlugDoesNotExist_ShouldReturnNotFound()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.GetAsync($"/api/resumes/public/does-not-exist-{Guid.NewGuid():N}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion
}
