using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.CredentialAccounts;

namespace ThaiX.Client.Services.CredentialAccounts;

public interface ICredentialAccountService
{
    Task<PagedApiResponse<CredentialAccountDto>> GetListAsync(CredentialAccountsListRequest request, CancellationToken cancellationToken = default);

    Task<CredentialAccountDetailDto> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Guid> CreateAsync(CreateCredentialAccountRequest request, CancellationToken cancellationToken = default);

    Task UpdateAsync(Guid id, UpdateCredentialAccountRequest request, CancellationToken cancellationToken = default);

    Task ChangePasswordAsync(Guid id, ChangeCredentialAccountPasswordRequest request, CancellationToken cancellationToken = default);

    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    Task MarkUsedAsync(Guid id, CancellationToken cancellationToken = default);

    Task ResetUsedAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CredentialAccountPasswordDto> ViewPasswordAsync(Guid id, CancellationToken cancellationToken = default);

    Task<CredentialAccountPasswordDto> CopyPasswordAsync(Guid id, CancellationToken cancellationToken = default);

    Task<PagedApiResponse<CredentialAccountAuditDto>> GetAuditLogsAsync(
        Guid credentialAccountId,
        CredentialAccountAuditListRequest request,
        CancellationToken cancellationToken = default);
}
