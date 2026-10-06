using System.Globalization;
using System.Net.Http.Json;
using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.Contacts;
using ThaiX.Client.Services.Api;
using ThaiX.Client.Services.Caching;

namespace ThaiX.Client.Services.Contacts;

/// <summary>
/// HTTP client implementation for contact API.
/// </summary>
public sealed class ContactService : IContactService
{
    private const string ContactsBase = "api/contacts";

    private readonly HttpClient _httpClient;
    private readonly IClientCacheService _cache;

    public ContactService(HttpClient httpClient, IClientCacheService cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public Task<PagedApiResponse<ContactListItemDto>> GetContactsAsync(
        ContactListRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = BuildListPath(request);
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.Contact,
            path,
            ct => FetchContactsAsync(path, ct),
            cancellationToken: cancellationToken);
    }

    public Task<ContactDetailDto> GetContactByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.Contact,
            id.ToString(),
            async ct =>
            {
                using var response = await _httpClient.GetAsync($"{ContactsBase}/{id}", ct);
                return await ApiResponseReader.ReadSuccessDataAsync<ContactDetailDto>(response, ct);
            },
            cancellationToken: cancellationToken);
    }

    public async Task<Guid> CreateContactAsync(
        CreateContactRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(ContactsBase, request, cancellationToken);
        var id = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
        return id;
    }

    public async Task UpdateContactProfileAsync(
        Guid id,
        UpdateContactProfileRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{ContactsBase}/{id}", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task<string> UploadContactAvatarAsync(
        Guid id,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default)
    {
        using var content = new StreamContent(fileStream);
        using var form = new MultipartFormDataContent();
        form.Add(content, "file", fileName);
        using var response = await _httpClient.PostAsync($"{ContactsBase}/{id}/avatar", form, cancellationToken);
        var url = await ApiResponseReader.ReadSuccessDataAsync<string>(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
        return url;
    }

    public async Task RemoveContactAvatarAsync(Guid id, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{ContactsBase}/{id}/avatar", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task DeleteContactAsync(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{ContactsBase}/{id}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task ArchiveContactAsync(
        Guid id,
        bool archive,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PatchAsJsonAsync($"{ContactsBase}/{id}/archive", new ArchiveContactRequest
        {
            Archive = archive
        }, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task AddContactEmailAsync(
        Guid id,
        AddEmailRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{ContactsBase}/{id}/emails", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task RemoveContactEmailAsync(
        Guid id,
        Guid emailId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{ContactsBase}/{id}/emails/{emailId}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task SetContactEmailPrimaryAsync(
        Guid id,
        Guid emailId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PatchAsync($"{ContactsBase}/{id}/emails/{emailId}/primary", null, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task AddContactPhoneAsync(
        Guid id,
        AddPhoneRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{ContactsBase}/{id}/phones", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task RemoveContactPhoneAsync(
        Guid id,
        Guid phoneId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{ContactsBase}/{id}/phones/{phoneId}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task SetContactPhonePrimaryAsync(
        Guid id,
        Guid phoneId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PatchAsync($"{ContactsBase}/{id}/phones/{phoneId}/primary", null, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task AddContactAddressAsync(
        Guid id,
        AddAddressRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{ContactsBase}/{id}/addresses", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task RemoveContactAddressAsync(
        Guid id,
        Guid addressId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{ContactsBase}/{id}/addresses/{addressId}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task AddContactSocialLinkAsync(
        Guid id,
        AddSocialLinkRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{ContactsBase}/{id}/social-links", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task RemoveContactSocialLinkAsync(
        Guid id,
        Guid socialLinkId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{ContactsBase}/{id}/social-links/{socialLinkId}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task AddContactTagAsync(
        Guid id,
        AddTagRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{ContactsBase}/{id}/tags", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task RemoveContactTagAsync(
        Guid id,
        Guid tagId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{ContactsBase}/{id}/tags/{tagId}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task AddContactBankAccountAsync(
        Guid id,
        AddBankAccountRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{ContactsBase}/{id}/bank-accounts", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task RemoveContactBankAccountAsync(
        Guid id,
        Guid bankAccountId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{ContactsBase}/{id}/bank-accounts/{bankAccountId}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task AddContactIdentityDocumentAsync(
        Guid id,
        AddIdentityDocumentRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync($"{ContactsBase}/{id}/identity-documents", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task RemoveContactIdentityDocumentAsync(
        Guid id,
        Guid identityDocumentId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{ContactsBase}/{id}/identity-documents/{identityDocumentId}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task SetContactCustomFieldAsync(
        Guid id,
        SetCustomFieldRequest request,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PutAsJsonAsync($"{ContactsBase}/{id}/custom-fields", request, cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task RemoveContactCustomFieldAsync(
        Guid id,
        string key,
        CancellationToken cancellationToken = default)
    {
        var encodedKey = Uri.EscapeDataString(key);
        using var response = await _httpClient.DeleteAsync($"{ContactsBase}/{id}/custom-fields/{encodedKey}", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
    }

    public async Task<ContactExportFileResult> GetImportTemplateAsync(CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"{ContactsBase}/import/template", cancellationToken);
        if (!response.IsSuccessStatusCode)
            await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);

        var fileContent = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? "contacts_import_template.csv";
        var contentType = response.Content.Headers.ContentType?.ToString() ?? "text/csv";
        return new ContactExportFileResult { FileName = fileName, ContentType = contentType, FileContent = fileContent };
    }

    public async Task<ContactImportResultDto> ImportContactsAsync(
        Stream csvStream,
        string fileName,
        CancellationToken cancellationToken = default)
    {
        using var response = await PostFileAsync($"{ContactsBase}/import", csvStream, fileName, cancellationToken);
        var result = await ApiResponseReader.ReadSuccessDataAsync<ContactImportResultDto>(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
        return result;
    }

    public async Task<Guid> StartContactImportAsync(
        Stream csvStream,
        string fileName,
        int? batchSize = null,
        CancellationToken cancellationToken = default)
    {
        var url = $"{ContactsBase}/import-async";
        if (batchSize.HasValue)
            url += $"?batchSize={batchSize.Value.ToString(CultureInfo.InvariantCulture)}";

        using var response = await PostFileAsync(url, csvStream, fileName, cancellationToken);
        var jobId = await ApiResponseReader.ReadSuccessDataAsync<Guid>(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
        return jobId;
    }

    public async Task<ContactImportJobDto> GetContactImportJobAsync(
        Guid jobId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.GetAsync($"{ContactsBase}/import/jobs/{jobId}", cancellationToken);
        return await ApiResponseReader.ReadSuccessDataAsync<ContactImportJobDto>(response, cancellationToken);
    }

    public async Task<ContactExportFileResult> ExportContactsAsync(
        ContactExportRequest request,
        CancellationToken cancellationToken = default)
    {
        var path = BuildExportPath(request);
        using var response = await _httpClient.GetAsync(path, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
            throw new ApiException(
                code: "CLIENT_EXPORT_FAILED",
                message: $"Export request failed with status {(int)response.StatusCode}.",
                statusCode: response.StatusCode);
        }

        var fileContent = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        var fileName = response.Content.Headers.ContentDisposition?.FileNameStar
            ?? response.Content.Headers.ContentDisposition?.FileName?.Trim('"')
            ?? $"contacts_export_{DateTime.UtcNow:yyyyMMdd_HHmmss}.csv";

        var contentType = response.Content.Headers.ContentType?.ToString() ?? "text/csv";

        return new ContactExportFileResult
        {
            FileName = fileName,
            ContentType = contentType,
            FileContent = fileContent
        };
    }

    public Task<IReadOnlyList<SuggestedUserDto>> GetSuggestedUsersAsync(
        Guid contactId,
        CancellationToken cancellationToken = default)
    {
        return _cache.GetOrCreateGroupedAsync(
            CacheGroups.ContactSuggestedUsers,
            contactId.ToString(),
            async ct =>
            {
                using var response = await _httpClient.GetAsync($"{ContactsBase}/{contactId}/suggest-users", ct);
                var data = await ApiResponseReader.ReadSuccessDataAsync<List<SuggestedUserDto>>(response, ct);
                return data ?? (IReadOnlyList<SuggestedUserDto>)Array.Empty<SuggestedUserDto>();
            },
            absoluteExpiration: ClientCacheTtl.Polling,
            cancellationToken: cancellationToken);
    }

    public async Task LinkUserToContactAsync(
        Guid contactId,
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.PostAsJsonAsync(
            $"api/users/{userId}/link-contact",
            new { ContactId = contactId },
            cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.User, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.UserLinkedContact, cancellationToken);
    }

    public async Task UnlinkUserFromContactAsync(Guid contactId, CancellationToken cancellationToken = default)
    {
        using var response = await _httpClient.DeleteAsync($"{ContactsBase}/{contactId}/linked-user", cancellationToken);
        await ApiResponseReader.ReadSuccessAsync(response, cancellationToken);
        await InvalidateContactCachesAsync(cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.User, cancellationToken);
        await _cache.InvalidateGroupAsync(CacheGroups.UserLinkedContact, cancellationToken);
    }

    private async Task<PagedApiResponse<ContactListItemDto>> FetchContactsAsync(string path, CancellationToken cancellationToken)
    {
        using var response = await _httpClient.GetAsync(path, cancellationToken);
        return await ApiResponseReader.ReadPagedSuccessAsync<ContactListItemDto>(response, cancellationToken);
    }

    private Task InvalidateContactCachesAsync(CancellationToken cancellationToken) =>
        _cache.InvalidateGroupAsync(CacheGroups.Contact, cancellationToken);

    private static string BuildListPath(ContactListRequest request)
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

        if (request.IsArchived.HasValue)
            parts.Add($"isArchived={request.IsArchived.Value.ToString().ToLowerInvariant()}");

        if (!string.IsNullOrWhiteSpace(request.Tag))
            parts.Add($"tag={Uri.EscapeDataString(request.Tag.Trim())}");

        return $"{ContactsBase}/?{string.Join("&", parts)}";
    }

    private static string BuildExportPath(ContactExportRequest request)
    {
        var parts = new List<string>();

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            parts.Add($"search={Uri.EscapeDataString(request.SearchTerm.Trim())}");

        if (request.IsArchived.HasValue)
            parts.Add($"isArchived={request.IsArchived.Value.ToString().ToLowerInvariant()}");

        if (!string.IsNullOrWhiteSpace(request.Tag))
            parts.Add($"tag={Uri.EscapeDataString(request.Tag.Trim())}");

        if (parts.Count == 0)
            return $"{ContactsBase}/export";

        return $"{ContactsBase}/export?{string.Join("&", parts)}";
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
