using System.Globalization;
using System.Net.Http.Json;
using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.Portfolios;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.Portfolios;

public sealed class PortfolioService : IPortfolioService
{
    private const string BaseUrl = "api/portfolios";

    private readonly HttpClient _httpClient;
    private readonly IClientCacheService _cache;

    public PortfolioService(HttpClient httpClient, IClientCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public Task<PagedApiResponse<PortfolioListItemDto>> GetListAsync(
        PortfoliosListRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = BuildListPath(request);
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.Portfolio,
            path,
            ct => FetchListAsync(path, ct),
            cancellationToken: cancellationToken);
    }

    public Task<PortfolioDetailDto> GetDetailAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.Portfolio,
            $"detail:{id}",
            async ct =>
            {
                using var response = await _httpClient.GetAsync($"{BaseUrl}/{id}", ct);
                return await ApiResponseReader.ReadSuccessDataAsync<PortfolioDetailDto>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public async Task<Guid> CreateAsync(
        CreatePortfolioRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(BaseUrl, request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await InvalidatePortfolioCachesAsync(cancellationToken);
        return id;
    }

    public async Task UpdateAsync(
        Guid id,
        UpdatePortfolioRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{BaseUrl}/{id}", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidatePortfolioCachesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{BaseUrl}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidatePortfolioCachesAsync(cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.CryptoPosition, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.StockPosition, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.SavingPosition, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.PositionTransaction, cancellationToken);
    }

    public async Task<PortfolioExportFileResult> GetImportTemplateAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"{BaseUrl}/import/template", cancellationToken);
        response.EnsureSuccessStatusCode();

        var fileContent = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? "portfolios_import_template.csv";
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "text/csv";

        return new PortfolioExportFileResult
        {
            FileName = fileName,
            ContentType = contentType,
            FileContent = fileContent
        };
    }

    public async Task<PortfolioImportResultDto> ImportAsync(
        Stream csvStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        using var response = await PostFileAsync($"{BaseUrl}/import", csvStream, fileName, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<PortfolioImportResultDto>(response, cancellationToken);
        await InvalidatePortfolioCachesAsync(cancellationToken);
        return result;
    }

    public async Task<PortfolioExportFileResult> ExportAsync(
        PortfolioExportRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = BuildExportPath(request);
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        response.EnsureSuccessStatusCode();

        var fileContent = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? $"portfolios_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "text/csv";

        return new PortfolioExportFileResult
        {
            FileName = fileName,
            ContentType = contentType,
            FileContent = fileContent
        };
    }

    private async Task<PagedApiResponse<PortfolioListItemDto>> FetchListAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        return await ApiResponseReader.ReadPagedSuccessAsync<PortfolioListItemDto>(response, cancellationToken);
    }

    private Task InvalidatePortfolioCachesAsync(CancellationToken cancellationToken) =>
        _cache.InvalidateGroupAsync(CacheGroups.Portfolio, cancellationToken);

    private static string BuildListPath(PortfoliosListRequest request)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(CultureInfo.InvariantCulture,
            $"{BaseUrl}?pageNumber={request.PageNumber}&pageSize={request.PageSize}");

        if (!string.IsNullOrWhiteSpace(request.SortBy))
            sb.Append($"&sortBy={Uri.EscapeDataString(request.SortBy)}&sortDescending={request.SortDescending.ToString().ToLowerInvariant()}");
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            sb.Append($"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}");

        return sb.ToString();
    }

    private static string BuildExportPath(PortfolioExportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.SearchTerm))
        {
            return $"{BaseUrl}/export";
        }

        return $"{BaseUrl}/export?searchTerm={Uri.EscapeDataString(request.SearchTerm.Trim())}";
    }

    private async Task<HttpResponseMessage> PostFileAsync(
        string url,
        Stream stream,
        string fileName,
        CancellationToken cancellationToken)
    {
        using var content = new StreamContent(stream);
        using var form = new MultipartFormDataContent();
        form.Add(content, "file", fileName);
        return await _httpClient.PostAsync(url, form, cancellationToken);
    }
}
