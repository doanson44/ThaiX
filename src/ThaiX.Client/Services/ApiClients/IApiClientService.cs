using ThaiX.Client.Models.ApiClients;

namespace ThaiX.Client.Services.ApiClients;

public interface IApiClientService
{
    Task<IReadOnlyList<ApiClientDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task<ApiClientDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<string>> GetAvailableScopesAsync(CancellationToken cancellationToken = default);

    Task<CreateApiClientResponse> CreateAsync(
        CreateApiClientRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiClientDto> UpdateAsync(
        Guid id,
        UpdateApiClientRequest request,
        CancellationToken cancellationToken = default);

    Task<ApiClientDto> UpdateScopesAsync(
        Guid id,
        UpdateApiClientScopesRequest request,
        CancellationToken cancellationToken = default);

    Task<RegenerateApiClientSecretResponse> RegenerateSecretAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<ApiClientDto> ActivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ApiClientDto> DeactivateAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
