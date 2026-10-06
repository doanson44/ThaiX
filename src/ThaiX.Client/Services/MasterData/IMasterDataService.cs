using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.MasterData;

namespace ThaiX.Client.Services.MasterData;

/// <summary>
/// Client service for master data API (Countries, Cities, Districts, Banks).
/// </summary>
public interface IMasterDataService
{
    Task<PagedApiResponse<CountryListItemDto>> GetCountriesAsync(
        MasterDataListRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedApiResponse<CityListItemDto>> GetCitiesAsync(
        MasterDataListWithParentRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedApiResponse<DistrictListItemDto>> GetDistrictsAsync(
        MasterDataListWithParentRequest request,
        CancellationToken cancellationToken = default);

    Task<PagedApiResponse<BankListItemDto>> GetBanksAsync(
        MasterDataListWithParentRequest request,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateCountryAsync(UpdateMasterDataRequest request, CancellationToken cancellationToken = default);
    Task UpdateCountryAsync(Guid id, UpdateMasterDataRequest request, CancellationToken cancellationToken = default);
    Task DeleteCountryAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ImportResultDto> ImportCountriesAsync(Stream csvStream, string fileName, CancellationToken cancellationToken = default);

    Task<Guid> CreateCityAsync(CreateCityRequest request, CancellationToken cancellationToken = default);
    Task UpdateCityAsync(Guid id, UpdateChildMasterDataRequest request, CancellationToken cancellationToken = default);
    Task DeleteCityAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ImportResultDto> ImportCitiesAsync(Stream csvStream, string fileName, CancellationToken cancellationToken = default);

    Task<Guid> CreateDistrictAsync(CreateDistrictRequest request, CancellationToken cancellationToken = default);
    Task UpdateDistrictAsync(Guid id, UpdateChildMasterDataRequest request, CancellationToken cancellationToken = default);
    Task DeleteDistrictAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ImportResultDto> ImportDistrictsAsync(Stream csvStream, string fileName, CancellationToken cancellationToken = default);

    Task<Guid> CreateBankAsync(CreateBankRequest request, CancellationToken cancellationToken = default);
    Task UpdateBankAsync(Guid id, UpdateChildMasterDataRequest request, CancellationToken cancellationToken = default);
    Task DeleteBankAsync(Guid id, CancellationToken cancellationToken = default);
    Task<ImportResultDto> ImportBanksAsync(Stream csvStream, string fileName, CancellationToken cancellationToken = default);

    Task<SymbolSearchResultDto> SearchSymbolsAsync(
        string assetType,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default);
}
