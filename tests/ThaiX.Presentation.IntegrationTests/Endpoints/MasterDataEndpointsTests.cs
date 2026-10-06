using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.MasterData.Banks.Queries.GetBanks;
using ThaiX.Application.Features.MasterData.Cities.Queries.GetCities;
using ThaiX.Application.Features.MasterData.Countries.Queries.GetCountries;
using ThaiX.Application.Features.MasterData.Districts.Queries.GetDistricts;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class MasterDataEndpointsTests : IntegrationTestBase
{
    public MasterDataEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetCountries_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/master-data/countries");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetCountries_WithoutMasterDataReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"masterdata-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/master-data/countries");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetCountries_WithMasterDataReadPermission_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryCode = GenerateCode("C", 6);
        var countryId = await CreateCountryAsync(countryCode, "Vietnam Test");

        // Act
        var response = await Client.GetAsync("/api/master-data/countries?search=Vietnam%20Test&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadPagedResponseAsync<CountryListItemDto>(response);
        envelope.Data.Should().NotBeNullOrEmpty();
        envelope.Data!.Single(x => x.Id == countryId).Code.Should().Be(countryCode);
    }

    [Fact]
    public async Task CreateCountry_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/master-data/countries", new
        {
            Code = "US",
            Name = "United States"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var envelope = await ReadResponseAsync<Guid>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task CreateCountry_WithDuplicateCode_ShouldReturnConflict()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var code = GenerateCode("J", 6);
        await CreateCountryAsync(code, "Japan");

        // Act
        var response = await Client.PostAsJsonAsync("/api/master-data/countries", new
        {
            Code = code,
            Name = "Japan Duplicate"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task CreateCountry_WithInvalidData_ShouldReturnBadRequest()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/master-data/countries", new
        {
            Code = new string('X', 11),
            Name = string.Empty
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task UpdateCountry_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var code = GenerateCode("S", 6);
        var countryId = await CreateCountryAsync(code, "Singapore");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/master-data/countries/{countryId}", new
        {
            Code = code,
            Name = "Singapore Updated"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var listResponse = await Client.GetAsync("/api/master-data/countries?search=Updated&pageNumber=1&pageSize=10");
        var list = await ReadPagedResponseAsync<CountryListItemDto>(listResponse);
        list.Data!.Single(x => x.Id == countryId).Name.Should().Be("Singapore Updated");
    }

    [Fact]
    public async Task UpdateCountry_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/master-data/countries/{Guid.NewGuid()}", new
        {
            Code = "TH",
            Name = "Thailand"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteCountry_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryId = await CreateCountryAsync(GenerateCode("M", 6), "Malaysia");

        // Act
        var response = await Client.DeleteAsync($"/api/master-data/countries/{countryId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var listResponse = await Client.GetAsync("/api/master-data/countries?search=Malaysia&pageNumber=1&pageSize=10");
        var list = await ReadPagedResponseAsync<CountryListItemDto>(listResponse);
        list.Data!.Any(x => x.Id == countryId).Should().BeFalse();
    }

    [Fact]
    public async Task CreateCity_WithValidCountry_ShouldReturnCreated()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryCode = GenerateCode("C", 6);
        await CreateCountryAsync(countryCode, "Vietnam City");
        var cityCode = GenerateCode("CT", 8);

        // Act
        var response = await Client.PostAsJsonAsync("/api/master-data/cities", new
        {
            Code = cityCode,
            Name = "Ho Chi Minh City",
            CountryCode = countryCode
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateCity_WithUnknownCountry_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/master-data/cities", new
        {
            Code = "HCM",
            Name = "Ho Chi Minh City",
            CountryCode = "ZZ"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetCities_WithParentFilter_ShouldReturnFilteredItems()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryCode = GenerateCode("C", 6);
        await CreateCountryAsync(countryCode, "Vietnam Filter");
        var cityId = await CreateCityAsync(GenerateCode("CI", 8), "Ha Noi", countryCode);

        // Act
        var response = await Client.GetAsync($"/api/master-data/cities?parentCode={countryCode}&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadPagedResponseAsync<CityListItemDto>(response);
        envelope.Data!.Single(x => x.Id == cityId).CountryCode.Should().Be(countryCode);
    }

    [Fact]
    public async Task CreateDistrict_WithValidCity_ShouldReturnCreated()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryCode = GenerateCode("C", 6);
        await CreateCountryAsync(countryCode, "Vietnam District");
        var cityCode = GenerateCode("CI", 8);
        await CreateCityAsync(cityCode, "Da Nang", countryCode);
        var districtCode = GenerateCode("D", 8);

        // Act
        var response = await Client.PostAsJsonAsync("/api/master-data/districts", new
        {
            Code = districtCode,
            Name = "Hai Chau",
            CityCode = cityCode
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateDistrict_WithUnknownCity_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/master-data/districts", new
        {
            Code = "HC",
            Name = "Hai Chau",
            CityCode = "UNKNOWN"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDistricts_WithParentFilter_ShouldReturnFilteredItems()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryCode = GenerateCode("C", 6);
        await CreateCountryAsync(countryCode, "Vietnam District Filter");
        var cityCode = GenerateCode("CI", 8);
        await CreateCityAsync(cityCode, "Can Tho", countryCode);
        var districtId = await CreateDistrictAsync(GenerateCode("D", 8), "Ninh Kieu", cityCode);

        // Act
        var response = await Client.GetAsync($"/api/master-data/districts?parentCode={cityCode}&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadPagedResponseAsync<DistrictListItemDto>(response);
        envelope.Data!.Single(x => x.Id == districtId).CityCode.Should().Be(cityCode);
    }

    [Fact]
    public async Task CreateBank_WithValidCountry_ShouldReturnCreated()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryCode = GenerateCode("C", 6);
        await CreateCountryAsync(countryCode, "Vietnam Bank");
        var bankCode = GenerateCode("B", 8);

        // Act
        var response = await Client.PostAsJsonAsync("/api/master-data/banks", new
        {
            Code = bankCode,
            Name = "Vietcombank",
            CountryCode = countryCode
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task CreateBank_WithUnknownCountry_ShouldReturnNotFound()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/master-data/banks", new
        {
            Code = "VCB",
            Name = "Vietcombank",
            CountryCode = "ZZ"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetBanks_WithParentFilter_ShouldReturnFilteredItems()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryCode = GenerateCode("C", 6);
        await CreateCountryAsync(countryCode, "Vietnam Bank Filter");
        var bankId = await CreateBankAsync(GenerateCode("B", 8), "Asia Commercial Bank", countryCode);

        // Act
        var response = await Client.GetAsync($"/api/master-data/banks?parentCode={countryCode}&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadPagedResponseAsync<BankListItemDto>(response);
        envelope.Data!.Single(x => x.Id == bankId).CountryCode.Should().Be(countryCode);
    }

    [Fact]
    public async Task UpdateCity_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryCode = GenerateCode("C", 6);
        await CreateCountryAsync(countryCode, "City Update Country");
        var cityCode = GenerateCode("CI", 8);
        var cityId = await CreateCityAsync(cityCode, "Before City", countryCode);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/master-data/cities/{cityId}", new
        {
            Code = cityCode,
            Name = "After City",
            ParentCode = countryCode
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var listResponse = await Client.GetAsync($"/api/master-data/cities?parentCode={countryCode}&search=After&pageNumber=1&pageSize=10");
        var list = await ReadPagedResponseAsync<CityListItemDto>(listResponse);
        list.Data!.Single(x => x.Id == cityId).Name.Should().Be("After City");
    }

    [Fact]
    public async Task DeleteCity_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryCode = GenerateCode("C", 6);
        await CreateCountryAsync(countryCode, "City Delete Country");
        var cityId = await CreateCityAsync(GenerateCode("CI", 8), "Delete City", countryCode);

        // Act
        var response = await Client.DeleteAsync($"/api/master-data/cities/{cityId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var listResponse = await Client.GetAsync($"/api/master-data/cities?parentCode={countryCode}&search=Delete&pageNumber=1&pageSize=10");
        var list = await ReadPagedResponseAsync<CityListItemDto>(listResponse);
        list.Data!.Any(x => x.Id == cityId).Should().BeFalse();
    }

    [Fact]
    public async Task ImportCities_WithValidCsv_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryCode = GenerateCode("C", 6);
        await CreateCountryAsync(countryCode, "City Import Country");
        var cityCode = GenerateCode("CI", 8);

        using var content = CreateFileFormData("file", "cities.csv", "text/csv",
            $"Code,Name,CountryCode\n{cityCode},Imported City,{countryCode}\n");

        // Act
        var response = await Client.PostAsync("/api/master-data/cities/import", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<ImportResult>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.InsertedCount.Should().Be(1);

        var listResponse = await Client.GetAsync($"/api/master-data/cities?parentCode={countryCode}&search=Imported&pageNumber=1&pageSize=10");
        var list = await ReadPagedResponseAsync<CityListItemDto>(listResponse);
        list.Data!.Single(x => x.Code == cityCode).Name.Should().Be("Imported City");
    }

    [Fact]
    public async Task UpdateDistrict_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryCode = GenerateCode("C", 6);
        await CreateCountryAsync(countryCode, "District Update Country");
        var cityCode = GenerateCode("CI", 8);
        await CreateCityAsync(cityCode, "District City", countryCode);
        var districtCode = GenerateCode("D", 8);
        var districtId = await CreateDistrictAsync(districtCode, "Before District", cityCode);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/master-data/districts/{districtId}", new
        {
            Code = districtCode,
            Name = "After District",
            ParentCode = cityCode
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var listResponse = await Client.GetAsync($"/api/master-data/districts?parentCode={cityCode}&search=After&pageNumber=1&pageSize=10");
        var list = await ReadPagedResponseAsync<DistrictListItemDto>(listResponse);
        list.Data!.Single(x => x.Id == districtId).Name.Should().Be("After District");
    }

    [Fact]
    public async Task DeleteDistrict_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryCode = GenerateCode("C", 6);
        await CreateCountryAsync(countryCode, "District Delete Country");
        var cityCode = GenerateCode("CI", 8);
        await CreateCityAsync(cityCode, "Delete District City", countryCode);
        var districtId = await CreateDistrictAsync(GenerateCode("D", 8), "Delete District", cityCode);

        // Act
        var response = await Client.DeleteAsync($"/api/master-data/districts/{districtId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var listResponse = await Client.GetAsync($"/api/master-data/districts?parentCode={cityCode}&search=Delete&pageNumber=1&pageSize=10");
        var list = await ReadPagedResponseAsync<DistrictListItemDto>(listResponse);
        list.Data!.Any(x => x.Id == districtId).Should().BeFalse();
    }

    [Fact]
    public async Task ImportDistricts_WithValidCsv_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryCode = GenerateCode("C", 6);
        await CreateCountryAsync(countryCode, "District Import Country");
        var cityCode = GenerateCode("CI", 8);
        await CreateCityAsync(cityCode, "Import District City", countryCode);
        var districtCode = GenerateCode("D", 8);

        using var content = CreateFileFormData("file", "districts.csv", "text/csv",
            $"Code,Name,CityCode\n{districtCode},Imported District,{cityCode}\n");

        // Act
        var response = await Client.PostAsync("/api/master-data/districts/import", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<ImportResult>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.InsertedCount.Should().Be(1);

        var listResponse = await Client.GetAsync($"/api/master-data/districts?parentCode={cityCode}&search=Imported&pageNumber=1&pageSize=10");
        var list = await ReadPagedResponseAsync<DistrictListItemDto>(listResponse);
        list.Data!.Single(x => x.Code == districtCode).Name.Should().Be("Imported District");
    }

    [Fact]
    public async Task UpdateBank_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryCode = GenerateCode("C", 6);
        await CreateCountryAsync(countryCode, "Bank Update Country");
        var bankCode = GenerateCode("B", 8);
        var bankId = await CreateBankAsync(bankCode, "Before Bank", countryCode);

        // Act
        var response = await Client.PutAsJsonAsync($"/api/master-data/banks/{bankId}", new
        {
            Code = bankCode,
            Name = "After Bank",
            ParentCode = countryCode
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var listResponse = await Client.GetAsync($"/api/master-data/banks?parentCode={countryCode}&search=After&pageNumber=1&pageSize=10");
        var list = await ReadPagedResponseAsync<BankListItemDto>(listResponse);
        list.Data!.Single(x => x.Id == bankId).Name.Should().Be("After Bank");
    }

    [Fact]
    public async Task DeleteBank_WithValidId_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryCode = GenerateCode("C", 6);
        await CreateCountryAsync(countryCode, "Bank Delete Country");
        var bankId = await CreateBankAsync(GenerateCode("B", 8), "Delete Bank", countryCode);

        // Act
        var response = await Client.DeleteAsync($"/api/master-data/banks/{bankId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var listResponse = await Client.GetAsync($"/api/master-data/banks?parentCode={countryCode}&search=Delete&pageNumber=1&pageSize=10");
        var list = await ReadPagedResponseAsync<BankListItemDto>(listResponse);
        list.Data!.Any(x => x.Id == bankId).Should().BeFalse();
    }

    [Fact]
    public async Task ImportBanks_WithValidCsv_ShouldReturnOk()
    {
        // Arrange
        await CreateAndAuthenticateAdminAsync();
        var countryCode = GenerateCode("C", 6);
        await CreateCountryAsync(countryCode, "Bank Import Country");
        var bankCode = GenerateCode("B", 8);

        using var content = CreateFileFormData("file", "banks.csv", "text/csv",
            $"Code,Name,CountryCode\n{bankCode},Imported Bank,{countryCode}\n");

        // Act
        var response = await Client.PostAsync("/api/master-data/banks/import", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<ImportResult>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data!.InsertedCount.Should().Be(1);

        var listResponse = await Client.GetAsync($"/api/master-data/banks?parentCode={countryCode}&search=Imported&pageNumber=1&pageSize=10");
        var list = await ReadPagedResponseAsync<BankListItemDto>(listResponse);
        list.Data!.Single(x => x.Code == bankCode).Name.Should().Be("Imported Bank");
    }

    private async Task<Guid> CreateCountryAsync(string code, string name)
    {
        var response = await Client.PostAsJsonAsync("/api/master-data/countries", new { Code = code, Name = name });
        response.EnsureSuccessStatusCode();
        var envelope = await ReadResponseAsync<Guid>(response);
        return envelope.Data;
    }

    private async Task<Guid> CreateCityAsync(string code, string name, string countryCode)
    {
        var response = await Client.PostAsJsonAsync("/api/master-data/cities", new { Code = code, Name = name, CountryCode = countryCode });
        response.EnsureSuccessStatusCode();
        var envelope = await ReadResponseAsync<Guid>(response);
        return envelope.Data;
    }

    private async Task<Guid> CreateDistrictAsync(string code, string name, string cityCode)
    {
        var response = await Client.PostAsJsonAsync("/api/master-data/districts", new { Code = code, Name = name, CityCode = cityCode });
        response.EnsureSuccessStatusCode();
        var envelope = await ReadResponseAsync<Guid>(response);
        return envelope.Data;
    }

    private async Task<Guid> CreateBankAsync(string code, string name, string countryCode)
    {
        var response = await Client.PostAsJsonAsync("/api/master-data/banks", new { Code = code, Name = name, CountryCode = countryCode });
        response.EnsureSuccessStatusCode();
        var envelope = await ReadResponseAsync<Guid>(response);
        return envelope.Data;
    }

    private static MultipartFormDataContent CreateFileFormData(string fieldName, string fileName, string contentType, string content)
    {
        var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, fieldName, fileName);
        return form;
    }

    private static async Task<PagedApiResponse<T>> ReadPagedResponseAsync<T>(HttpResponseMessage response)
    {
        var envelope = await response.Content.ReadFromJsonAsync<PagedApiResponse<T>>(JsonOptions);
        return envelope ?? throw new InvalidOperationException("Unable to deserialize paged API response.");
    }

    private static string GenerateCode(string prefix, int totalLength)
    {
        var suffixLength = Math.Max(1, totalLength - prefix.Length);
        var suffix = Guid.NewGuid().ToString("N")[..suffixLength].ToUpperInvariant();
        return (prefix + suffix)[..totalLength].ToUpperInvariant();
    }
}
