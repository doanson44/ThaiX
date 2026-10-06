using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using ThaiX.Application.Features.Portfolios.Queries.GetPortfolioDetail;
using ThaiX.Application.Features.Portfolios.Queries.GetPortfolios;
using ThaiX.Domain.Aggregates.Portfolios;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class PortfolioEndpointsTests : IntegrationTestBase
{
    public PortfolioEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    #region Get List

    [Fact]
    public async Task GetPortfolios_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/portfolios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPortfolios_WithoutPortfolioReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"portfolio-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/portfolios");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPortfolios_WithPortfolioReadPermission_ShouldReturnOk()
    {
        // Arrange
        var email = $"portfolio-reader-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.PortfolioRead, Permissions.PortfolioWrite });
        await AuthenticateAsync(email, "Test@Pass123");
        var createdPortfolioId = await CreatePortfolioAsync("Alpha Portfolio", PortfolioType.Trading);
        await CreatePortfolioAsync("Beta Portfolio", PortfolioType.Savings);

        // Act
        var response = await Client.GetAsync("/api/portfolios?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ReadPagedResponseAsync<PortfolioListItemDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNullOrEmpty();
        envelope.Data!.Any(item => item.Id == createdPortfolioId).Should().BeTrue();
    }

    [Fact]
    public async Task GetPortfolios_WithSearchTerm_ShouldReturnFilteredItems()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var matchedName = $"Searchable Portfolio {Guid.NewGuid():N}";
        var unmatchedName = $"Other Portfolio {Guid.NewGuid():N}";
        await CreatePortfolioAsync(matchedName, PortfolioType.Trading);
        await CreatePortfolioAsync(unmatchedName, PortfolioType.Savings);

        // Act
        var response = await Client.GetAsync($"/api/portfolios?searchTerm={Uri.EscapeDataString(matchedName)}&pageNumber=1&pageSize=20");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ReadPagedResponseAsync<PortfolioListItemDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNullOrEmpty();
        envelope.Data!.All(item => item.Name.Contains(matchedName, StringComparison.OrdinalIgnoreCase)).Should().BeTrue();
    }

    #endregion

    #region Get Detail

    [Fact]
    public async Task GetPortfolioById_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var portfolioId = Guid.NewGuid();

        // Act
        var response = await Client.GetAsync($"/api/portfolios/{portfolioId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPortfolioById_WithoutPortfolioReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"portfolio-detail-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync($"/api/portfolios/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetPortfolioById_WithPortfolioReadPermission_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Detail Portfolio", PortfolioType.Retirement, "Detail description");

        // Act
        var response = await Client.GetAsync($"/api/portfolios/{portfolioId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var envelope = await ReadResponseAsync<PortfolioDetailDto>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.Id.Should().Be(portfolioId);
        envelope.Data.Name.Should().Be("Detail Portfolio");
        envelope.Data.Description.Should().Be("Detail description");
    }

    [Fact]
    public async Task GetPortfolioById_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.GetAsync($"/api/portfolios/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Create

    [Fact]
    public async Task CreatePortfolio_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync("/api/portfolios", new
        {
            Name = "New Portfolio",
            PortfolioType = PortfolioType.Trading,
            Description = "Created without auth"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreatePortfolio_WithoutPortfolioWritePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"portfolio-create-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PostAsJsonAsync("/api/portfolios", new
        {
            Name = "New Portfolio",
            PortfolioType = PortfolioType.Trading,
            Description = "Created without permission"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreatePortfolio_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var request = new
        {
            Name = $"Created Portfolio {Guid.NewGuid():N}",
            PortfolioType = PortfolioType.LongTerm,
            Description = "Integration test portfolio"
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/portfolios", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var envelope = await ReadResponseAsync<Guid>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreatePortfolio_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var request = new
        {
            Name = string.Empty,
            PortfolioType = 0,
            Description = new string('x', 501)
        };

        // Act
        var response = await Client.PostAsJsonAsync("/api/portfolios", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var envelope = await ReadResponseAsync<object>(response);
        envelope.Success.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Details.Should().NotBeNullOrEmpty();
    }

    #endregion

    #region Update

    [Fact]
    public async Task UpdatePortfolio_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PutAsJsonAsync($"/api/portfolios/{Guid.NewGuid()}", new
        {
            Name = "Updated Portfolio",
            PortfolioType = PortfolioType.Savings,
            Description = "Updated without auth"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task UpdatePortfolio_WithoutPortfolioWritePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"portfolio-update-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/portfolios/{Guid.NewGuid()}", new
        {
            Name = "Updated Portfolio",
            PortfolioType = PortfolioType.Savings,
            Description = "Updated without permission"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task UpdatePortfolio_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Before Update", PortfolioType.Trading, "Before");
        var request = new
        {
            Name = "After Update",
            PortfolioType = PortfolioType.Retirement,
            Description = "After"
        };

        // Act
        var response = await Client.PutAsJsonAsync($"/api/portfolios/{portfolioId}", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var detailResponse = await Client.GetAsync($"/api/portfolios/{portfolioId}");
        detailResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await ReadResponseAsync<PortfolioDetailDto>(detailResponse);
        detail.Data!.Name.Should().Be("After Update");
        detail.Data.PortfolioType.Should().Be(PortfolioType.Retirement);
        detail.Data.Description.Should().Be("After");
    }

    [Fact]
    public async Task UpdatePortfolio_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Valid Portfolio", PortfolioType.Trading);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/portfolios/{portfolioId}", new
        {
            Name = string.Empty,
            PortfolioType = 0,
            Description = new string('x', 501)
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
        var envelope = await ReadResponseAsync<object>(response);
        envelope.Success.Should().BeFalse();
        envelope.Error.Should().NotBeNull();
        envelope.Error!.Details.Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task UpdatePortfolio_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/portfolios/{Guid.NewGuid()}", new
        {
            Name = "Updated Portfolio",
            PortfolioType = PortfolioType.Trading,
            Description = "Not found"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Delete

    [Fact]
    public async Task DeletePortfolio_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.DeleteAsync($"/api/portfolios/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task DeletePortfolio_WithoutPortfolioDeletePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"portfolio-delete-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.DeleteAsync($"/api/portfolios/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeletePortfolio_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var portfolioId = await CreatePortfolioAsync("Delete Portfolio", PortfolioType.Savings);

        // Act
        var response = await Client.DeleteAsync($"/api/portfolios/{portfolioId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var detailResponse = await Client.GetAsync($"/api/portfolios/{portfolioId}");
        detailResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeletePortfolio_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.DeleteAsync($"/api/portfolios/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    #endregion

    #region Import / Export

    // ---- template ----

    [Fact]
    public async Task GetPortfolioImportTemplate_WithPortfolioReadPermission_ShouldReturnCsvTemplate()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.GetAsync("/api/portfolios/import/template");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Name");
        body.Should().Contain("PortfolioType");
        body.Should().Contain("Description");
    }

    [Fact]
    public async Task GetPortfolioImportTemplate_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/portfolios/import/template");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetPortfolioImportTemplate_WithoutPortfolioReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"portfolio-tmpl-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/portfolios/import/template");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ---- export ----

    [Fact]
    public async Task ExportPortfolios_WithPortfolioReadPermission_ShouldReturnCsvFile()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var name = $"ExportPortfolio{Guid.NewGuid():N}";
        await CreatePortfolioAsync(name, PortfolioType.Trading, "Exported desc");

        // Act
        var response = await Client.GetAsync($"/api/portfolios/export?searchTerm={name}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Name");
        body.Should().Contain("PortfolioType");
        body.Should().Contain(name);
    }

    [Fact]
    public async Task ExportPortfolios_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/portfolios/export");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExportPortfolios_WithoutPortfolioReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"portfolio-export-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/portfolios/export");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ExportPortfolios_WithSearchFilter_ShouldReturnOnlyMatchingRows()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var matchName = $"MatchExport{Guid.NewGuid():N}";
        var otherName = $"OtherExport{Guid.NewGuid():N}";
        await CreatePortfolioAsync(matchName, PortfolioType.Trading);
        await CreatePortfolioAsync(otherName, PortfolioType.Savings);

        // Act
        var response = await Client.GetAsync($"/api/portfolios/export?searchTerm={matchName}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain(matchName);
        body.Should().NotContain(otherName);
    }

    [Fact]
    public async Task ExportPortfolios_WithNoPortfolios_ShouldReturnEmptyCsvWithHeaders()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var uniqueTerm = $"NoSuchPortfolio{Guid.NewGuid():N}";

        // Act
        var response = await Client.GetAsync($"/api/portfolios/export?searchTerm={uniqueTerm}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Name");
        // Only the header row -- no data rows
        body.Trim().Split('\n').Length.Should().Be(1);
    }

    // ---- import ----

    [Fact]
    public async Task ImportPortfolios_WithValidCsv_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var uniqueName = $"ImportedPortfolio{Guid.NewGuid():N}";
        var csv = $"Name,PortfolioType,Description\n{uniqueName},Trading,Imported via CSV\n";
        using var content = CreateFileFormData("file", "portfolios.csv", "text/csv", csv);

        // Act
        var response = await Client.PostAsync("/api/portfolios/import", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, responseBody);
        var envelope = await ReadResponseAsync<object>(response);
        envelope.Success.Should().BeTrue();
        responseBody.Should().Contain("insertedCount");
    }

    [Fact]
    public async Task ImportPortfolios_WithMultipleRows_ShouldInsertAll()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var name1 = $"Batch1{Guid.NewGuid():N}";
        var name2 = $"Batch2{Guid.NewGuid():N}";
        var csv = $"Name,PortfolioType,Description\n{name1},Trading,First\n{name2},Savings,Second\n";
        using var content = CreateFileFormData("file", "portfolios-batch.csv", "text/csv", csv);

        // Act
        var response = await Client.PostAsync("/api/portfolios/import", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, responseBody);
        responseBody.Should().Contain("insertedCount");

        // Verify portfolios exist
        var listResponse = await Client.GetAsync($"/api/portfolios?pageSize=100");
        var body = await listResponse.Content.ReadAsStringAsync();
        body.Should().Contain(name1);
        body.Should().Contain(name2);
    }

    [Fact]
    public async Task ImportPortfolios_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var csv = "Name,PortfolioType\nTest,Trading\n";
        using var content = CreateFileFormData("file", "portfolios.csv", "text/csv", csv);

        // Act
        var response = await Client.PostAsync("/api/portfolios/import", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ImportPortfolios_WithoutPortfolioWritePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"portfolio-import-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");
        var csv = "Name,PortfolioType\nTest,Trading\n";
        using var content = CreateFileFormData("file", "portfolios.csv", "text/csv", csv);

        // Act
        var response = await Client.PostAsync("/api/portfolios/import", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ImportPortfolios_WithEmptyCsv_ShouldReturnOkWithErrors()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var csv = string.Empty;
        using var content = CreateFileFormData("file", "empty.csv", "text/csv", csv);

        // Act
        var response = await Client.PostAsync("/api/portfolios/import", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        // Empty CSV returns OK with errors reported in the ImportResult
        response.StatusCode.Should().Be(HttpStatusCode.OK, responseBody);
        responseBody.Should().Contain("insertedCount");
    }

    [Fact]
    public async Task ImportPortfolios_WithDuplicateName_ShouldUpdateExistingPortfolio()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var name = $"DuplicateImport{Guid.NewGuid():N}";
        await CreatePortfolioAsync(name, PortfolioType.Trading, "Original");
        var csv = $"Name,PortfolioType,Description\n{name},Savings,Updated via Import\n";
        using var content = CreateFileFormData("file", "duplicate.csv", "text/csv", csv);

        // Act
        var response = await Client.PostAsync("/api/portfolios/import", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, responseBody);
        responseBody.Should().Contain("updatedCount");
    }

    #endregion

    private async Task<Guid> CreatePortfolioAsync(
        string name,
        PortfolioType portfolioType,
        string? description = null)
    {
        var response = await Client.PostAsJsonAsync("/api/portfolios", new
        {
            Name = name,
            PortfolioType = portfolioType,
            Description = description
        });

        response.EnsureSuccessStatusCode();
        var envelope = await ReadResponseAsync<Guid>(response);
        return envelope.Data;
    }

    private static async Task<PagedApiResponse<T>> ReadPagedResponseAsync<T>(HttpResponseMessage response)
    {
        var envelope = await response.Content.ReadFromJsonAsync<PagedApiResponse<T>>(JsonOptions);
        if (envelope is null)
        {
            throw new InvalidOperationException("Unable to deserialize paged API response.");
        }

        return envelope;
    }

    private static MultipartFormDataContent CreateFileFormData(string fieldName, string fileName, string contentType, string content)
    {
        var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, fieldName, fileName);
        return form;
    }
}
