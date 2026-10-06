using System.Globalization;
using System.Net.Http.Json;
using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.AssetPositions;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.AssetPositions;

public sealed class AssetPositionService : IAssetPositionService
{
    private const string CryptoBase = "api/crypto-positions";
    private const string StockBase = "api/stock-positions";
    private const string SavingBase = "api/saving-positions";

    private readonly HttpClient _httpClient;
    private readonly IClientCacheService _cache;

    public AssetPositionService(HttpClient httpClient, IClientCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public Task<PagedApiResponse<CryptoPositionListItemDto>> GetCryptoPositionsAsync(
        CryptoPositionsListRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = BuildCryptoListPath(request);
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.CryptoPosition,
            path,
            ct => FetchPagedAsync<CryptoPositionListItemDto>(path, ct),
            cancellationToken: cancellationToken);
    }

    public async Task<Guid> CreateCryptoPositionAsync(
        CreateCryptoPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(CryptoBase, request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await InvalidateCryptoCachesAsync(cancellationToken);
        await InvalidatePortfolioCachesAsync(cancellationToken);
        return id;
    }

    public async Task UpdateCryptoPositionTargetsAsync(
        Guid id,
        UpdateCryptoPositionTargetsRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{CryptoBase}/{id}/targets", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateCryptoCachesAsync(cancellationToken);
        await InvalidatePortfolioCachesAsync(cancellationToken);
    }

    public async Task AddCryptoTransactionAsync(
        Guid positionId,
        AddCryptoTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{CryptoBase}/{positionId}/transactions", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateCryptoCachesAsync(cancellationToken);
        await InvalidateTransactionCachesAsync(cancellationToken);
        await InvalidatePortfolioCachesAsync(cancellationToken);
    }

    public async Task DeleteCryptoPositionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{CryptoBase}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateCryptoCachesAsync(cancellationToken);
        await InvalidatePortfolioCachesAsync(cancellationToken);
    }

    public Task<PagedApiResponse<StockPositionListItemDto>> GetStockPositionsAsync(
        StockPositionsListRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = BuildStockListPath(request);
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.StockPosition,
            path,
            ct => FetchPagedAsync<StockPositionListItemDto>(path, ct),
            cancellationToken: cancellationToken);
    }

    public async Task<Guid> CreateStockPositionAsync(
        CreateStockPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(StockBase, request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await InvalidateStockCachesAsync(cancellationToken);
        await InvalidatePortfolioCachesAsync(cancellationToken);
        return id;
    }

    public async Task UpdateStockPositionTargetsAsync(
        Guid id,
        UpdateStockPositionTargetsRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{StockBase}/{id}/targets", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateStockCachesAsync(cancellationToken);
        await InvalidatePortfolioCachesAsync(cancellationToken);
    }

    public async Task AddStockTransactionAsync(
        Guid positionId,
        AddStockTransactionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{StockBase}/{positionId}/transactions", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateStockCachesAsync(cancellationToken);
        await InvalidateTransactionCachesAsync(cancellationToken);
        await InvalidatePortfolioCachesAsync(cancellationToken);
    }

    public async Task DeleteStockPositionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{StockBase}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateStockCachesAsync(cancellationToken);
        await InvalidatePortfolioCachesAsync(cancellationToken);
    }

    public Task<PagedApiResponse<SavingPositionListItemDto>> GetSavingPositionsAsync(
        SavingPositionsListRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = BuildSavingListPath(request);
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.SavingPosition,
            path,
            ct => FetchPagedAsync<SavingPositionListItemDto>(path, ct),
            cancellationToken: cancellationToken);
    }

    public async Task<Guid> CreateSavingPositionAsync(
        CreateSavingPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(SavingBase, request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await InvalidateSavingCachesAsync(cancellationToken);
        await InvalidatePortfolioCachesAsync(cancellationToken);
        return id;
    }

    public async Task UpdateSavingPositionAsync(
        Guid id,
        UpdateSavingPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{SavingBase}/{id}", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateSavingCachesAsync(cancellationToken);
        await InvalidatePortfolioCachesAsync(cancellationToken);
    }

    public async Task WithdrawSavingPositionAsync(
        Guid id,
        WithdrawSavingPositionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{SavingBase}/{id}/withdraw", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateSavingCachesAsync(cancellationToken);
        await InvalidatePortfolioCachesAsync(cancellationToken);
    }

    public async Task DeleteSavingPositionAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{SavingBase}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateSavingCachesAsync(cancellationToken);
        await InvalidatePortfolioCachesAsync(cancellationToken);
    }

    public Task<PagedApiResponse<PositionTransactionDto>> GetTransactionsAsync(
        TransactionsListRequest request,
        CancellationToken cancellationToken = default)
    {
        string baseUrl = request.AssetType == "Stock" ? StockBase : CryptoBase;
        var url = $"{baseUrl}/{request.PositionId}/transactions?pageNumber={request.PageNumber}&pageSize={request.PageSize}";

        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.PositionTransaction,
            url,
            ct => FetchPagedAsync<PositionTransactionDto>(url, ct),
            cancellationToken: cancellationToken);
    }

    private async Task<PagedApiResponse<T>> FetchPagedAsync<T>(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        return await ApiResponseReader.ReadPagedSuccessAsync<T>(response, cancellationToken);
    }

    private Task InvalidateCryptoCachesAsync(CancellationToken cancellationToken) =>
        _cache.InvalidateGroupAsync(CacheGroups.CryptoPosition, cancellationToken);

    private Task InvalidateStockCachesAsync(CancellationToken cancellationToken) =>
        _cache.InvalidateGroupAsync(CacheGroups.StockPosition, cancellationToken);

    private Task InvalidateSavingCachesAsync(CancellationToken cancellationToken) =>
        _cache.InvalidateGroupAsync(CacheGroups.SavingPosition, cancellationToken);

    private Task InvalidateTransactionCachesAsync(CancellationToken cancellationToken) =>
        _cache.InvalidateGroupAsync(CacheGroups.PositionTransaction, cancellationToken);

    private Task InvalidatePortfolioCachesAsync(CancellationToken cancellationToken) =>
        _cache.InvalidateGroupAsync(CacheGroups.Portfolio, cancellationToken);

    private static string BuildCryptoListPath(CryptoPositionsListRequest request)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(CultureInfo.InvariantCulture,
            $"{CryptoBase}?portfolioId={request.PortfolioId}&pageNumber={request.PageNumber}&pageSize={request.PageSize}");
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            sb.Append($"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}");
        if (request.IsClosed.HasValue)
            sb.Append($"&isClosed={request.IsClosed.Value.ToString().ToLowerInvariant()}");
        return sb.ToString();
    }

    private static string BuildStockListPath(StockPositionsListRequest request)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(CultureInfo.InvariantCulture,
            $"{StockBase}?portfolioId={request.PortfolioId}&pageNumber={request.PageNumber}&pageSize={request.PageSize}");
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            sb.Append($"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}");
        if (request.IsClosed.HasValue)
            sb.Append($"&isClosed={request.IsClosed.Value.ToString().ToLowerInvariant()}");
        return sb.ToString();
    }

    private static string BuildSavingListPath(SavingPositionsListRequest request)
    {
        var sb = new System.Text.StringBuilder();
        sb.Append(CultureInfo.InvariantCulture,
            $"{SavingBase}?portfolioId={request.PortfolioId}&pageNumber={request.PageNumber}&pageSize={request.PageSize}");
        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            sb.Append($"&searchTerm={Uri.EscapeDataString(request.SearchTerm)}");
        if (!string.IsNullOrWhiteSpace(request.Status))
            sb.Append($"&status={Uri.EscapeDataString(request.Status)}");
        return sb.ToString();
    }
}
