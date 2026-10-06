using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.AssetPositions;

namespace ThaiX.Client.Services.AssetPositions;

public interface IAssetPositionService
{
    // Crypto
    Task<PagedApiResponse<CryptoPositionListItemDto>> GetCryptoPositionsAsync(
        CryptoPositionsListRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateCryptoPositionAsync(
        CreateCryptoPositionRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateCryptoPositionTargetsAsync(
        Guid id,
        UpdateCryptoPositionTargetsRequest request,
        CancellationToken cancellationToken = default);

    Task AddCryptoTransactionAsync(
        Guid positionId,
        AddCryptoTransactionRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteCryptoPositionAsync(Guid id, CancellationToken cancellationToken = default);

    // Stock
    Task<PagedApiResponse<StockPositionListItemDto>> GetStockPositionsAsync(
        StockPositionsListRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateStockPositionAsync(
        CreateStockPositionRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateStockPositionTargetsAsync(
        Guid id,
        UpdateStockPositionTargetsRequest request,
        CancellationToken cancellationToken = default);

    Task AddStockTransactionAsync(
        Guid positionId,
        AddStockTransactionRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteStockPositionAsync(Guid id, CancellationToken cancellationToken = default);

    // Saving
    Task<PagedApiResponse<SavingPositionListItemDto>> GetSavingPositionsAsync(
        SavingPositionsListRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateSavingPositionAsync(
        CreateSavingPositionRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateSavingPositionAsync(
        Guid id,
        UpdateSavingPositionRequest request,
        CancellationToken cancellationToken = default);

    Task WithdrawSavingPositionAsync(
        Guid id,
        WithdrawSavingPositionRequest request,
        CancellationToken cancellationToken = default);

    Task DeleteSavingPositionAsync(Guid id, CancellationToken cancellationToken = default);

    // Transactions
    Task<PagedApiResponse<PositionTransactionDto>> GetTransactionsAsync(
        TransactionsListRequest request,
        CancellationToken cancellationToken = default);
}
