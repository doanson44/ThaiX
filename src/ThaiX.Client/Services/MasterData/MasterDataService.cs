using System.Globalization;
using System.Net.Http.Json;
using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.MasterData;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.MasterData;

/// <summary>
/// HTTP client implementation for master data API.
/// </summary>
public sealed class MasterDataService : IMasterDataService
{
    private const string CountriesBase = "api/master-data/countries";
    private const string CitiesBase = "api/master-data/cities";
    private const string DistrictsBase = "api/master-data/districts";
    private const string BanksBase = "api/master-data/banks";

    private readonly HttpClient _httpClient;
    private readonly IClientCacheService _cache;

    public MasterDataService(HttpClient httpClient, IClientCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public Task<PagedApiResponse<CountryListItemDto>> GetCountriesAsync(
        MasterDataListRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = BuildListPath(CountriesBase, request, null);
        var cacheKey = $"{CacheGroups.Country}:{path}";
        return _cache.GetOrCreateAsync(
            cacheKey,
            ct => FetchPagedAsync<CountryListItemDto>(path, ct),
            group: CacheGroups.Country,
            cancellationToken: cancellationToken);
    }

    public Task<PagedApiResponse<CityListItemDto>> GetCitiesAsync(
        MasterDataListWithParentRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = BuildListPath(CitiesBase, request, request.ParentCode);
        var cacheKey = $"{CacheGroups.City}:{path}";
        return _cache.GetOrCreateAsync(
            cacheKey,
            ct => FetchPagedAsync<CityListItemDto>(path, ct),
            group: CacheGroups.City,
            cancellationToken: cancellationToken);
    }

    public Task<PagedApiResponse<DistrictListItemDto>> GetDistrictsAsync(
        MasterDataListWithParentRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = BuildListPath(DistrictsBase, request, request.ParentCode);
        var cacheKey = $"{CacheGroups.District}:{path}";
        return _cache.GetOrCreateAsync(
            cacheKey,
            ct => FetchPagedAsync<DistrictListItemDto>(path, ct),
            group: CacheGroups.District,
            cancellationToken: cancellationToken);
    }

    public Task<PagedApiResponse<BankListItemDto>> GetBanksAsync(
        MasterDataListWithParentRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = BuildListPath(BanksBase, request, request.ParentCode);
        var cacheKey = $"{CacheGroups.Bank}:{path}";
        return _cache.GetOrCreateAsync(
            cacheKey,
            ct => FetchPagedAsync<BankListItemDto>(path, ct),
            group: CacheGroups.Bank,
            cancellationToken: cancellationToken);
    }

    public async Task<Guid> CreateCountryAsync(UpdateMasterDataRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(CountriesBase, request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.Country, cancellationToken);
        return id;
    }

    public async Task UpdateCountryAsync(Guid id, UpdateMasterDataRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{CountriesBase}/{id}", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.Country, cancellationToken);
    }

    public async Task DeleteCountryAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{CountriesBase}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.Country, cancellationToken);
    }

    public async Task<ImportResultDto> ImportCountriesAsync(Stream csvStream, string fileName, CancellationToken cancellationToken = default)
    {
        using var response = await PostFileAsync($"{CountriesBase}/import", csvStream, fileName, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<ImportResultDto>(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.Country, cancellationToken);
        return result;
    }

    public async Task<Guid> CreateCityAsync(CreateCityRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(CitiesBase, request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.City, cancellationToken);
        return id;
    }

    public async Task UpdateCityAsync(Guid id, UpdateChildMasterDataRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{CitiesBase}/{id}", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.City, cancellationToken);
    }

    public async Task DeleteCityAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{CitiesBase}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.City, cancellationToken);
    }

    public async Task<ImportResultDto> ImportCitiesAsync(Stream csvStream, string fileName, CancellationToken cancellationToken = default)
    {
        using var response = await PostFileAsync($"{CitiesBase}/import", csvStream, fileName, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<ImportResultDto>(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.City, cancellationToken);
        return result;
    }

    public async Task<Guid> CreateDistrictAsync(CreateDistrictRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(DistrictsBase, request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.District, cancellationToken);
        return id;
    }

    public async Task UpdateDistrictAsync(Guid id, UpdateChildMasterDataRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{DistrictsBase}/{id}", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.District, cancellationToken);
    }

    public async Task DeleteDistrictAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{DistrictsBase}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.District, cancellationToken);
    }

    public async Task<ImportResultDto> ImportDistrictsAsync(Stream csvStream, string fileName, CancellationToken cancellationToken = default)
    {
        using var response = await PostFileAsync($"{DistrictsBase}/import", csvStream, fileName, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<ImportResultDto>(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.District, cancellationToken);
        return result;
    }

    public async Task<Guid> CreateBankAsync(CreateBankRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(BanksBase, request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.Bank, cancellationToken);
        return id;
    }

    public async Task UpdateBankAsync(Guid id, UpdateChildMasterDataRequest request, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{BanksBase}/{id}", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.Bank, cancellationToken);
    }

    public async Task DeleteBankAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{BanksBase}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.Bank, cancellationToken);
    }

    public async Task<ImportResultDto> ImportBanksAsync(Stream csvStream, string fileName, CancellationToken cancellationToken = default)
    {
        using var response = await PostFileAsync($"{BanksBase}/import", csvStream, fileName, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<ImportResultDto>(response, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.Bank, cancellationToken);
        return result;
    }

    private async Task<PagedApiResponse<T>> FetchPagedAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        return await ApiResponseReader.ReadPagedSuccessAsync<T>(response, cancellationToken);
    }

    private static string BuildListPath(string basePath, MasterDataListRequest request, string? parentCode)
    {
        var parts = new List<string>
        {
            $"pageNumber={request.PageNumber.ToString(CultureInfo.InvariantCulture)}",
            $"pageSize={request.PageSize.ToString(CultureInfo.InvariantCulture)}",
            $"sortBy={Uri.EscapeDataString(request.SortBy)}",
            $"sortDescending={request.SortDescending.ToString().ToLowerInvariant()}"
        };
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            parts.Add($"search={Uri.EscapeDataString(request.SearchTerm.Trim())}");
        if (!string.IsNullOrWhiteSpace(parentCode))
            parts.Add($"parentCode={Uri.EscapeDataString(parentCode.Trim())}");
        return $"{basePath}/?{string.Join("&", parts)}";
    }

    private async Task<HttpResponseMessage> PostFileAsync(string url, Stream stream, string fileName, CancellationToken cancellationToken)
    {
        using var content = new StreamContent(stream);
        using var form = new MultipartFormDataContent();
        form.Add(content, "file", fileName);
        return await _httpClient.PostAsync(url, form, cancellationToken);
    }

    public Task<SymbolSearchResultDto> SearchSymbolsAsync(
        string assetType,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var queryParts = new List<string>
        {
            $"assetType={Uri.EscapeDataString(assetType)}",
            $"page={page.ToString(CultureInfo.InvariantCulture)}",
            $"pageSize={pageSize.ToString(CultureInfo.InvariantCulture)}"
        };
        if (!string.IsNullOrWhiteSpace(search))
            queryParts.Add($"search={Uri.EscapeDataString(search.Trim())}");

        var url = $"api/master-data/symbols/search?{string.Join("&", queryParts)}";
        return _cache.GetOrCreateAsync(
            $"{CacheGroups.MasterDataSymbol}:{url}",
            async ct =>
            {
                using var response = await _httpClient.GetAsync(url, ct);
                return await ApiResponseReader.ReadSuccessDataAsync<SymbolSearchResultDto>(response, ct);
            },
            group: CacheGroups.MasterDataSymbol,
            cancellationToken: cancellationToken);
    }
}
