using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text;
using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.JsonBins;
using ThaiX.Client.Services.Api;

namespace ThaiX.Client.Services.JsonBins;

public sealed class JsonBinService : IJsonBinService
{
    private readonly HttpClient _http;

    public JsonBinService(HttpClient http)
    {
        _http = http;
    }

    public async Task<PagedApiResponse<JsonBinListItemModel>> SearchAsync(
        JsonBinSearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var url = $"api/json-bins{BuildSearchQuery(request)}";
        using var response = await _http.GetAsync(url, cancellationToken);
        return await ApiResponseReader.ReadPagedSuccessAsync<JsonBinListItemModel>(response, cancellationToken);
    }

    public async Task<JsonBinDetailModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync($"api/json-bins/{id}", cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        return await ApiResponseReader.ReadSuccessDataAsync<JsonBinDetailModel>(response, cancellationToken);
    }

    public async Task<JsonBinDetailModel?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync(
            $"api/json-bins/by-code/{Uri.EscapeDataString(code)}",
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        return await ApiResponseReader.ReadSuccessDataAsync<JsonBinDetailModel>(response, cancellationToken);
    }

    public async Task<byte[]?> GetContentAsync(
        Guid id,
        bool decompress = true,
        CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync(
            $"api/json-bins/{id}/content?decompress={decompress}",
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsByteArrayAsync(cancellationToken);
    }

    public async Task<string> CreateAsync(CreateJsonBinModel model, CancellationToken cancellationToken = default)
    {
        using var response = await _http.PostAsJsonAsync("api/json-bins", model, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<string>(response, cancellationToken);
    }

    public async Task<string> UpdateAsync(Guid id, UpdateJsonBinModel model, CancellationToken cancellationToken = default)
    {
        using var response = await _http.PutAsJsonAsync($"api/json-bins/{id}", model, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<string>(response, cancellationToken);
    }

    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _http.DeleteAsync($"api/json-bins/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }

    public async Task ExpireAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _http.PostAsync($"api/json-bins/{id}/expire", content: null, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }

    public async Task<ValidateJsonBinCodeModel> ValidateCodeAsync(
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder($"api/json-bins/code-exists?code={Uri.EscapeDataString(code)}");
        if (excludeId.HasValue)
            sb.Append(CultureInfo.InvariantCulture, $"&excludeId={excludeId.Value}");

        using var response = await _http.GetAsync(sb.ToString(), cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<ValidateJsonBinCodeModel>(response, cancellationToken);
    }

    public async Task<GenerateShareLinkResultModel> GenerateShareLinkAsync(
        Guid id,
        DateTime? shareExpiresAtUtc = null,
        CancellationToken cancellationToken = default)
    {
        var body = new GenerateShareLinkRequestModel { ShareExpiresAtUtc = shareExpiresAtUtc };
        using var response = await _http.PostAsJsonAsync($"api/json-bins/{id}/share", body, cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<GenerateShareLinkResultModel>(response, cancellationToken);
    }

    public async Task RevokeShareLinkAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _http.DeleteAsync($"api/json-bins/{id}/share", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
    }

    public async Task<JsonBinSharedModel?> GetSharedAsync(string token, CancellationToken cancellationToken = default)
    {
        using var response = await _http.GetAsync(
            $"api/public/json-bins/share/{Uri.EscapeDataString(token)}",
            cancellationToken);
        if (response.StatusCode == HttpStatusCode.NotFound)
            return null;

        return await ApiResponseReader.ReadSuccessDataAsync<JsonBinSharedModel>(response, cancellationToken);
    }

    private static string BuildSearchQuery(JsonBinSearchRequest request)
    {
        var sb = new StringBuilder();
        sb.Append(CultureInfo.InvariantCulture, $"?pageNumber={request.PageNumber}&pageSize={request.PageSize}");
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            sb.Append(CultureInfo.InvariantCulture, $"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}");
        if (request.Category.HasValue)
            sb.Append(CultureInfo.InvariantCulture, $"&category={(int)request.Category.Value}");
        if (request.ReferenceId.HasValue)
            sb.Append(CultureInfo.InvariantCulture, $"&referenceId={request.ReferenceId.Value}");
        if (request.IncludeExpired.HasValue)
            sb.Append(CultureInfo.InvariantCulture, $"&includeExpired={request.IncludeExpired.Value.ToString().ToLowerInvariant()}");
        if (!string.IsNullOrWhiteSpace(request.SortBy))
            sb.Append(CultureInfo.InvariantCulture, $"&sortBy={Uri.EscapeDataString(request.SortBy)}");
        if (request.SortDescending)
            sb.Append("&sortDescending=true");
        return sb.ToString();
    }
}
