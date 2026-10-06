using ThaiX.Client.Models.Api;
using ThaiX.Client.Models.Contacts;

namespace ThaiX.Client.Services.Contacts;

/// <summary>
/// Client service for contact API.
/// </summary>
public interface IContactService
{
    Task<PagedApiResponse<ContactListItemDto>> GetContactsAsync(
        ContactListRequest request,
        CancellationToken cancellationToken = default);

    Task<ContactDetailDto> GetContactByIdAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task<Guid> CreateContactAsync(
        CreateContactRequest request,
        CancellationToken cancellationToken = default);

    Task UpdateContactProfileAsync(
        Guid id,
        UpdateContactProfileRequest request,
        CancellationToken cancellationToken = default);

    Task<string> UploadContactAvatarAsync(
        Guid id,
        Stream fileStream,
        string fileName,
        string contentType,
        CancellationToken cancellationToken = default);

    Task RemoveContactAvatarAsync(Guid id, CancellationToken cancellationToken = default);

    Task DeleteContactAsync(
        Guid id,
        CancellationToken cancellationToken = default);

    Task ArchiveContactAsync(
        Guid id,
        bool archive,
        CancellationToken cancellationToken = default);

    Task AddContactEmailAsync(
        Guid id,
        AddEmailRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveContactEmailAsync(
        Guid id,
        Guid emailId,
        CancellationToken cancellationToken = default);

    Task SetContactEmailPrimaryAsync(
        Guid id,
        Guid emailId,
        CancellationToken cancellationToken = default);

    Task AddContactPhoneAsync(
        Guid id,
        AddPhoneRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveContactPhoneAsync(
        Guid id,
        Guid phoneId,
        CancellationToken cancellationToken = default);

    Task SetContactPhonePrimaryAsync(
        Guid id,
        Guid phoneId,
        CancellationToken cancellationToken = default);

    Task AddContactAddressAsync(
        Guid id,
        AddAddressRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveContactAddressAsync(
        Guid id,
        Guid addressId,
        CancellationToken cancellationToken = default);

    Task AddContactSocialLinkAsync(
        Guid id,
        AddSocialLinkRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveContactSocialLinkAsync(
        Guid id,
        Guid socialLinkId,
        CancellationToken cancellationToken = default);

    Task AddContactTagAsync(
        Guid id,
        AddTagRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveContactTagAsync(
        Guid id,
        Guid tagId,
        CancellationToken cancellationToken = default);

    Task AddContactBankAccountAsync(
        Guid id,
        AddBankAccountRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveContactBankAccountAsync(
        Guid id,
        Guid bankAccountId,
        CancellationToken cancellationToken = default);

    Task AddContactIdentityDocumentAsync(
        Guid id,
        AddIdentityDocumentRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveContactIdentityDocumentAsync(
        Guid id,
        Guid identityDocumentId,
        CancellationToken cancellationToken = default);

    Task SetContactCustomFieldAsync(
        Guid id,
        SetCustomFieldRequest request,
        CancellationToken cancellationToken = default);

    Task RemoveContactCustomFieldAsync(
        Guid id,
        string key,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Downloads the standard CSV import template. Convert your CSV to this format before importing.
    /// </summary>
    Task<ContactExportFileResult> GetImportTemplateAsync(CancellationToken cancellationToken = default);

    Task<ContactImportResultDto> ImportContactsAsync(
        Stream csvStream,
        string fileName,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts asynchronous contact import. Poll <see cref="GetContactImportJobAsync"/> for status.
    /// </summary>
    Task<Guid> StartContactImportAsync(
        Stream csvStream,
        string fileName,
        int? batchSize = null,
        CancellationToken cancellationToken = default);

    Task<ContactImportJobDto> GetContactImportJobAsync(
        Guid jobId,
        CancellationToken cancellationToken = default);

    Task<ContactExportFileResult> ExportContactsAsync(
        ContactExportRequest request,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets users suggested for linking to this contact (by matching contact phone numbers).
    /// </summary>
    Task<IReadOnlyList<SuggestedUserDto>> GetSuggestedUsersAsync(
        Guid contactId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Links an Identity user to this contact (creates UserContactLink and UserProfile).
    /// </summary>
    Task LinkUserToContactAsync(
        Guid contactId,
        Guid userId,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Unlinks the user from this contact (removes UserContactLink and UserProfile).
    /// </summary>
    Task UnlinkUserFromContactAsync(Guid contactId, CancellationToken cancellationToken = default);
}
