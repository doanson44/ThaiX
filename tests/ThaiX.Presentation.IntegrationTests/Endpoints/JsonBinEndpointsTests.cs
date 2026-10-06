using System.Net;
using System.Net.Http.Json;
using ThaiX.Application.Features.JsonBins.Commands.CreateJsonBin;
using ThaiX.Application.Features.JsonBins.Commands.GenerateJsonBinShareLink;
using ThaiX.Application.Features.JsonBins.Queries.GetJsonBinById;
using ThaiX.Domain.Aggregates.JsonBin;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class JsonBinEndpointsTests : IntegrationTestBase
{
    public JsonBinEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    private static async Task<T?> ReadDataAsync<T>(HttpResponseMessage response)
    {
        var envelope = await response.Content.ReadFromJsonAsync<ApiResponse<T>>(JsonOptions);
        return envelope is not null ? envelope.Data : default;
    }

    #region Create

    [Fact]
    public async Task Create_ShouldReturnCode_WhenAuthorized()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/json-bins", new CreateJsonBinCommand
        {
            Code = "IT_CREATE",
            Name = "it-create",
            Category = JsonBinCategories.Temp,
            ContentJson = "{\"ok\":true}"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var code = await ReadDataAsync<string>(response);
        code.Should().Be("IT_CREATE");
    }

    [Fact]
    public async Task Create_ShouldAutoGenerateCategoryPrefixedCode_WhenCodeOmitted()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/json-bins", new CreateJsonBinCommand
        {
            Name = "auto",
            Category = JsonBinCategories.Config,
            ContentJson = "{}"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var code = await ReadDataAsync<string>(response);
        code.Should().StartWith("CONFIG_");
    }

    [Fact]
    public async Task Create_ShouldReturnValidationError_WhenNameEmpty()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await Client.PostAsJsonAsync("/api/json-bins", new CreateJsonBinCommand
        {
            Name = "",
            ContentJson = "{}"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    #endregion

    #region GetByCode

    [Fact]
    public async Task GetByCode_ShouldReturnDetail()
    {
        await CreateAndAuthenticateAdminAsync();

        var create = await Client.PostAsJsonAsync("/api/json-bins", new CreateJsonBinCommand
        {
            Code = "IT_BY_CODE",
            Name = "by-code",
            ContentJson = "{\"v\":1}"
        });
        var code = await ReadDataAsync<string>(create);

        var response = await Client.GetAsync($"/api/json-bins/by-code/{code}");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region CodeExists

    [Fact]
    public async Task CodeExists_ShouldReturnExistsTrue_WhenCodeTaken()
    {
        await CreateAndAuthenticateAdminAsync();

        await Client.PostAsJsonAsync("/api/json-bins", new CreateJsonBinCommand
        {
            Code = "IT_DUP",
            Name = "dup",
            ContentJson = "{}"
        });

        var response = await Client.GetAsync("/api/json-bins/code-exists?code=it_dup");
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Auth / Not Found

    [Fact]
    public async Task GetList_ShouldReturnUnauthorized_WhenAnonymous()
    {
        Client.DefaultRequestHeaders.Authorization = null;

        var response = await Client.GetAsync("/api/json-bins");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetList_ShouldReturnForbidden_WhenMissingPermission()
    {
        var email = $"jsonbin-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        var response = await Client.GetAsync("/api/json-bins");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetById_ShouldReturnNotFound_WhenMissing()
    {
        await CreateAndAuthenticateAdminAsync();

        var response = await Client.GetAsync($"/api/json-bins/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task Delete_ShouldSoftDelete()
    {
        await CreateAndAuthenticateAdminAsync();

        var create = await Client.PostAsJsonAsync("/api/json-bins", new CreateJsonBinCommand
        {
            Code = "IT_DELETE",
            Name = "it-delete",
            ContentJson = "{}"
        });
        var code = await ReadDataAsync<string>(create);
        var detailResponse = await Client.GetAsync($"/api/json-bins/by-code/{code}");
        var detail = await ReadDataAsync<JsonBinDetailDto>(detailResponse);
        detail.Should().NotBeNull();

        var response = await Client.DeleteAsync($"/api/json-bins/{detail!.Id}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var byCode = await Client.GetAsync($"/api/json-bins/by-code/{code}");
        byCode.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Share

    [Fact]
    public async Task Share_ShouldAllowAnonymousGet()
    {
        await CreateAndAuthenticateAdminAsync();

        var create = await Client.PostAsJsonAsync("/api/json-bins", new CreateJsonBinCommand
        {
            Code = "IT_SHARE",
            Name = "share-me",
            ContentJson = "{\"public\":true}"
        });
        var code = await ReadDataAsync<string>(create);
        var detailResponse = await Client.GetAsync($"/api/json-bins/by-code/{code}");
        var detail = await ReadDataAsync<JsonBinDetailDto>(detailResponse);
        detail.Should().NotBeNull();

        var share = await Client.PostAsJsonAsync($"/api/json-bins/{detail!.Id}/share", new { });
        share.StatusCode.Should().Be(HttpStatusCode.OK);
        var shareResult = await ReadDataAsync<GenerateJsonBinShareLinkResult>(share);
        shareResult.Should().NotBeNull();

        Client.DefaultRequestHeaders.Authorization = null;
        var publicResponse = await Client.GetAsync($"/api/public/json-bins/share/{shareResult!.Token}");
        publicResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion

    #region Expire

    [Fact]
    public async Task Expire_ShouldSucceed()
    {
        await CreateAndAuthenticateAdminAsync();

        var create = await Client.PostAsJsonAsync("/api/json-bins", new CreateJsonBinCommand
        {
            Code = "IT_EXPIRE",
            Name = "it-expire",
            ContentJson = "{}"
        });
        var code = await ReadDataAsync<string>(create);
        var detailResponse = await Client.GetAsync($"/api/json-bins/by-code/{code}");
        var detail = await ReadDataAsync<JsonBinDetailDto>(detailResponse);
        detail.Should().NotBeNull();

        var response = await Client.PostAsync($"/api/json-bins/{detail!.Id}/expire", content: null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    #endregion
}
