using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.JsonBins;

namespace ThaiX.Client.Services.JsonBins;

public interface IJsonBinService
{
    Task<PagedApiResponse<JsonBinListItemModel>> SearchAsync(
        JsonBinSearchRequest request,
        CancellationToken cancellationToken = default);

    Task<JsonBinDetailModel?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<JsonBinDetailModel?> GetByCodeAsync(string code, CancellationToken cancellationToken = default);

    Task<byte[]?> GetContentAsync(Guid id, bool decompress = true, CancellationToken cancellationToken = default);

    /// <summary>Creates a bin and returns the assigned/generated code.</summary>
    Task<string> CreateAsync(CreateJsonBinModel model, CancellationToken cancellationToken = default);

    /// <summary>Updates a bin and returns the resulting code.</summary>
    Task<string> UpdateAsync(Guid id, UpdateJsonBinModel model, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task ExpireAsync(Guid id, CancellationToken cancellationToken = default);

    Task<ValidateJsonBinCodeModel> ValidateCodeAsync(
        string code,
        Guid? excludeId = null,
        CancellationToken cancellationToken = default);

    Task<GenerateShareLinkResultModel> GenerateShareLinkAsync(
        Guid id,
        DateTime? shareExpiresAtUtc = null,
        CancellationToken cancellationToken = default);

    Task RevokeShareLinkAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>Anonymous public fetch -- no auth header required on the API.</summary>
    Task<JsonBinSharedModel?> GetSharedAsync(string token, CancellationToken cancellationToken = default);
}
