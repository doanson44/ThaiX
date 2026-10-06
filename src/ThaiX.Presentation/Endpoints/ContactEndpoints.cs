using MediatR;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.Contacts.Commands.AddContactAddress;
using ThaiX.Application.Features.Contacts.Commands.AddContactBankAccount;
using ThaiX.Application.Features.Contacts.Commands.AddContactEmail;
using ThaiX.Application.Features.Contacts.Commands.AddContactIdentityDocument;
using ThaiX.Application.Features.Contacts.Commands.AddContactPhone;
using ThaiX.Application.Features.Contacts.Commands.AddContactSocialLink;
using ThaiX.Application.Features.Contacts.Commands.AddContactTag;
using ThaiX.Application.Features.Contacts.Commands.ArchiveContact;
using ThaiX.Application.Features.Contacts.Commands.CreateContact;
using ThaiX.Application.Features.Contacts.Commands.DeleteContact;
using ThaiX.Application.Features.Contacts.Commands.ImportContacts;
using ThaiX.Application.Features.Contacts.Commands.RemoveContactAddress;
using ThaiX.Application.Features.Contacts.Commands.RemoveContactAvatar;
using ThaiX.Application.Features.Contacts.Commands.RemoveContactBankAccount;
using ThaiX.Application.Features.Contacts.Commands.RemoveContactCustomField;
using ThaiX.Application.Features.Contacts.Commands.RemoveContactEmail;
using ThaiX.Application.Features.Contacts.Commands.RemoveContactIdentityDocument;
using ThaiX.Application.Features.Contacts.Commands.RemoveContactPhone;
using ThaiX.Application.Features.Contacts.Commands.RemoveContactSocialLink;
using ThaiX.Application.Features.Contacts.Commands.RemoveContactTag;
using ThaiX.Application.Features.Contacts.Commands.SetContactCustomField;
using ThaiX.Application.Features.Contacts.Commands.SetContactEmailPrimary;
using ThaiX.Application.Features.Contacts.Commands.SetContactPhonePrimary;
using ThaiX.Application.Features.Contacts.Commands.StartContactImport;
using ThaiX.Application.Features.Contacts.Commands.UpdateContactProfile;
using ThaiX.Application.Features.Contacts.Commands.UploadContactAvatar;
using ThaiX.Application.Features.Contacts.Models;
using ThaiX.Application.Features.Contacts.Queries.ExportContacts;
using ThaiX.Application.Features.Contacts.Queries.GetContactById;
using ThaiX.Application.Features.Contacts.Queries.GetContactImportJob;
using ThaiX.Application.Features.Contacts.Queries.GetContacts;
using ThaiX.Application.Features.Identity.Commands.UnlinkUserFromContact;
using ThaiX.Application.Features.Identity.Dtos;
using ThaiX.Application.Features.Identity.Queries.SuggestUsersByContactPhones;
using ThaiX.Presentation.Extensions;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

/// <summary>
/// Contact management endpoints.
/// </summary>
public static class ContactEndpoints
{
    public static void MapContactEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/contacts")
            .WithTags("Contacts");

        #region Core CRUD

        // GET /api/contacts
        group.MapGet("/", async (
            [AsParameters] ContactQueryParameters parameters,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var query = new GetContactsQuery
            {
                PageNumber = parameters.PageNumber ?? 1,
                PageSize = parameters.PageSize ?? 10,
                SortBy = string.IsNullOrWhiteSpace(parameters.SortBy) ? "lastname" : parameters.SortBy.Trim(),
                SortDescending = parameters.SortDescending ?? false,
                SearchTerm = parameters.SearchTerm,
                IsArchived = parameters.IsArchived,
                Tag = parameters.Tag
            };

            var pagedResult = await mediator.Send(query, cancellationToken);
            var response = pagedResult.ToPagedApiResponse(httpContext.GetCorrelationId());
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactRead)
        .WithName("GetContacts")
        .WithDescription("Get paginated list of contacts with search, archive, and tag filters");

        // GET /api/contacts/{id}
        group.MapGet("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new GetContactByIdQuery { Id = id }, cancellationToken);
            var response = ApiResponse<ContactDetailDto>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactRead)
        .WithName("GetContactById")
        .WithDescription("Get a contact with all details");

        // GET /api/contacts/{id}/suggest-users - Suggest users to link by matching contact phones
        group.MapGet("/{id:guid}/suggest-users", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var list = await mediator.Send(new SuggestUsersByContactPhonesQuery(id), cancellationToken);
            var response = ApiResponse<IReadOnlyList<SuggestedUserDto>>.SuccessResult(list);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactRead)
        .WithName("SuggestUsersByContactPhones")
        .WithDescription("Get users suggested for linking by matching contact phone numbers");

        // DELETE /api/contacts/{id}/linked-user - Unlink user from contact
        group.MapDelete("/{id:guid}/linked-user", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new UnlinkUserFromContactCommand(id), cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("UnlinkUserFromContact")
        .WithDescription("Unlink the user from this contact");

        // POST /api/contacts
        group.MapPost("/", async (
            [FromBody] CreateContactCommand command,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var id = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<Guid>.SuccessResult(id);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Created($"/api/contacts/{id}", response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("CreateContact")
        .WithDescription("Create a new contact");

        // PUT /api/contacts/{id}
        group.MapPut("/{id:guid}", async (
            Guid id,
            [FromBody] UpdateContactProfileRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var command = new UpdateContactProfileCommand
            {
                Id = id,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Company = request.Company,
                JobTitle = request.JobTitle,
                AvatarUrl = request.AvatarUrl,
                Birthday = request.Birthday,
                Notes = request.Notes
            };

            await mediator.Send(command, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("UpdateContactProfile")
        .WithDescription("Update a contact's profile");

        // POST /api/contacts/{id}/avatar
        group.MapPost("/{id:guid}/avatar", async (
                Guid id,
                HttpContext httpContext,
                IMediator mediator,
                CancellationToken cancellationToken) =>
            {
                var form = await httpContext.Request.ReadFormAsync(cancellationToken);
                var file = form.Files.GetFile("file");
                if (file is null)
                    return Results.BadRequest("File is required.");
                var contentType = file.ContentType;
                if (string.IsNullOrWhiteSpace(contentType) && !string.IsNullOrEmpty(file.FileName))
                    contentType = InferImageContentType(file.FileName);
                if (string.IsNullOrWhiteSpace(contentType))
                    contentType = "application/octet-stream";
                await using var stream = file.OpenReadStream();
                var url = await mediator.Send(new UploadContactAvatarCommand
                {
                    ContactId = id,
                    FileStream = stream,
                    FileName = file.FileName,
                    ContentType = contentType
                }, cancellationToken);
                var response = ApiResponse<string>.SuccessResult(url);
                response.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.Ok(response);
            })
            .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
            .WithName("UploadContactAvatar")
            .WithDescription("Upload contact avatar (image/jpeg, image/png, image/webp; max 2 MB)")
            .DisableAntiforgery();

        // DELETE /api/contacts/{id}/avatar
        group.MapDelete("/{id:guid}/avatar", async (
                Guid id,
                IMediator mediator,
                HttpContext httpContext,
                CancellationToken cancellationToken) =>
            {
                await mediator.Send(new RemoveContactAvatarCommand { ContactId = id }, cancellationToken);
                var response = ApiResponse.SuccessResult();
                response.Metadata.CorrelationId = httpContext.GetCorrelationId();
                return Results.Ok(response);
            })
            .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
            .WithName("RemoveContactAvatar")
            .WithDescription("Remove contact avatar (delete file and clear URL)");

        // DELETE /api/contacts/{id}
        group.MapDelete("/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new DeleteContactCommand { Id = id }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactDelete)
        .WithName("DeleteContact")
        .WithDescription("Soft-delete a contact");

        // PATCH /api/contacts/{id}/archive
        group.MapPatch("/{id:guid}/archive", async (
            Guid id,
            [FromBody] ArchiveContactRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new ArchiveContactCommand { Id = id, Archive = request.Archive }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("ArchiveContact")
        .WithDescription("Archive or unarchive a contact");

        #endregion

        #region Emails

        // POST /api/contacts/{id}/emails
        group.MapPost("/{id:guid}/emails", async (
            Guid id,
            [FromBody] AddEmailRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new AddContactEmailCommand
            {
                ContactId = id,
                Value = request.Value,
                IsPrimary = request.IsPrimary
            }, cancellationToken);

            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("AddContactEmail")
        .WithDescription("Add an email to a contact");

        // DELETE /api/contacts/{id}/emails/{emailId}
        group.MapDelete("/{id:guid}/emails/{emailId:guid}", async (
            Guid id,
            Guid emailId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new RemoveContactEmailCommand { ContactId = id, EmailId = emailId }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("RemoveContactEmail")
        .WithDescription("Remove an email from a contact");

        // PATCH /api/contacts/{id}/emails/{emailId}/primary
        group.MapPatch("/{id:guid}/emails/{emailId:guid}/primary", async (
            Guid id,
            Guid emailId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new SetContactEmailPrimaryCommand { ContactId = id, EmailId = emailId }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("SetContactEmailPrimary")
        .WithDescription("Set an email as the primary email for the contact");

        #endregion

        #region Phones

        // POST /api/contacts/{id}/phones
        group.MapPost("/{id:guid}/phones", async (
            Guid id,
            [FromBody] AddPhoneRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new AddContactPhoneCommand
            {
                ContactId = id,
                Value = request.Value,
                IsPrimary = request.IsPrimary
            }, cancellationToken);

            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("AddContactPhone")
        .WithDescription("Add a phone number to a contact");

        // DELETE /api/contacts/{id}/phones/{phoneId}
        group.MapDelete("/{id:guid}/phones/{phoneId:guid}", async (
            Guid id,
            Guid phoneId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new RemoveContactPhoneCommand { ContactId = id, PhoneId = phoneId }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("RemoveContactPhone")
        .WithDescription("Remove a phone number from a contact");

        // PATCH /api/contacts/{id}/phones/{phoneId}/primary
        group.MapPatch("/{id:guid}/phones/{phoneId:guid}/primary", async (
            Guid id,
            Guid phoneId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new SetContactPhonePrimaryCommand { ContactId = id, PhoneId = phoneId }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("SetContactPhonePrimary")
        .WithDescription("Set a phone as the primary phone for the contact");

        #endregion

        #region Addresses

        // POST /api/contacts/{id}/addresses
        group.MapPost("/{id:guid}/addresses", async (
            Guid id,
            [FromBody] AddAddressRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new AddContactAddressCommand
            {
                ContactId = id,
                Street = request.Street,
                CountryCode = request.CountryCode,
                CityCode = request.CityCode,
                DistrictCode = request.DistrictCode,
                PostalCode = request.PostalCode,
                IsPrimary = request.IsPrimary
            }, cancellationToken);

            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("AddContactAddress")
        .WithDescription("Add an address to a contact");

        // DELETE /api/contacts/{id}/addresses/{addressId}
        group.MapDelete("/{id:guid}/addresses/{addressId:guid}", async (
            Guid id,
            Guid addressId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new RemoveContactAddressCommand { ContactId = id, AddressId = addressId }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("RemoveContactAddress")
        .WithDescription("Remove an address from a contact");

        #endregion

        #region Social Links

        // POST /api/contacts/{id}/social-links
        group.MapPost("/{id:guid}/social-links", async (
            Guid id,
            [FromBody] AddSocialLinkRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new AddContactSocialLinkCommand
            {
                ContactId = id,
                Platform = request.Platform,
                Url = request.Url
            }, cancellationToken);

            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("AddContactSocialLink")
        .WithDescription("Add a social media link to a contact");

        // DELETE /api/contacts/{id}/social-links/{socialLinkId}
        group.MapDelete("/{id:guid}/social-links/{socialLinkId:guid}", async (
            Guid id,
            Guid socialLinkId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new RemoveContactSocialLinkCommand { ContactId = id, SocialLinkId = socialLinkId }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("RemoveContactSocialLink")
        .WithDescription("Remove a social media link from a contact");

        #endregion

        #region Tags

        // POST /api/contacts/{id}/tags
        group.MapPost("/{id:guid}/tags", async (
            Guid id,
            [FromBody] AddTagRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new AddContactTagCommand
            {
                ContactId = id,
                Name = request.Name
            }, cancellationToken);

            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("AddContactTag")
        .WithDescription("Add a tag to a contact");

        // DELETE /api/contacts/{id}/tags/{tagId}
        group.MapDelete("/{id:guid}/tags/{tagId:guid}", async (
            Guid id,
            Guid tagId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new RemoveContactTagCommand { ContactId = id, TagId = tagId }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("RemoveContactTag")
        .WithDescription("Remove a tag from a contact");

        #endregion

        #region Bank Accounts

        // POST /api/contacts/{id}/bank-accounts
        group.MapPost("/{id:guid}/bank-accounts", async (
            Guid id,
            [FromBody] AddBankAccountRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new AddContactBankAccountCommand
            {
                ContactId = id,
                BankCode = request.BankCode,
                BranchName = request.BranchName,
                AccountNumber = request.AccountNumber,
                AccountName = request.AccountName,
                CurrencyCode = request.CurrencyCode,
                IsPrimary = request.IsPrimary
            }, cancellationToken);

            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("AddContactBankAccount")
        .WithDescription("Add a bank account to a contact");

        // DELETE /api/contacts/{id}/bank-accounts/{bankAccountId}
        group.MapDelete("/{id:guid}/bank-accounts/{bankAccountId:guid}", async (
            Guid id,
            Guid bankAccountId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new RemoveContactBankAccountCommand { ContactId = id, BankAccountId = bankAccountId }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("RemoveContactBankAccount")
        .WithDescription("Remove a bank account from a contact");

        #endregion

        #region Identity Documents

        // POST /api/contacts/{id}/identity-documents
        group.MapPost("/{id:guid}/identity-documents", async (
            Guid id,
            [FromBody] AddIdentityDocumentRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new AddContactIdentityDocumentCommand
            {
                ContactId = id,
                DocumentType = request.DocumentType,
                DocumentNumber = request.DocumentNumber,
                IssuedBy = request.IssuedBy,
                IssuedPlace = request.IssuedPlace,
                IssuedDate = request.IssuedDate,
                ExpiryDate = request.ExpiryDate
            }, cancellationToken);

            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("AddContactIdentityDocument")
        .WithDescription("Add an identity document to a contact");

        // DELETE /api/contacts/{id}/identity-documents/{identityDocumentId}
        group.MapDelete("/{id:guid}/identity-documents/{identityDocumentId:guid}", async (
            Guid id,
            Guid identityDocumentId,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new RemoveContactIdentityDocumentCommand { ContactId = id, IdentityDocumentId = identityDocumentId }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("RemoveContactIdentityDocument")
        .WithDescription("Remove an identity document from a contact");

        #endregion

        #region Custom Fields

        // PUT /api/contacts/{id}/custom-fields
        group.MapPut("/{id:guid}/custom-fields", async (
            Guid id,
            [FromBody] SetCustomFieldRequest request,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new SetContactCustomFieldCommand
            {
                ContactId = id,
                Key = request.Key,
                Value = request.Value
            }, cancellationToken);

            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("SetContactCustomField")
        .WithDescription("Set a custom field on a contact (upserts by key)");

        // DELETE /api/contacts/{id}/custom-fields/{key}
        group.MapDelete("/{id:guid}/custom-fields/{key}", async (
            Guid id,
            string key,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            await mediator.Send(new RemoveContactCustomFieldCommand { ContactId = id, Key = key }, cancellationToken);
            var response = ApiResponse.SuccessResult();
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .WithName("RemoveContactCustomField")
        .WithDescription("Remove a custom field from a contact");

        #endregion

        #region Import / Export

        // GET /api/contacts/import/template - Google Contacts compatible CSV template
        group.MapGet("/import/template", () =>
        {
            var headerLine = string.Join(ContactImportCsvSpec.Delimiter, ContactImportCsvSpec.GoogleTemplateHeaders);
            // Write UTF-8 BOM for Excel compatibility
            var bom = new byte[] { 0xEF, 0xBB, 0xBF };
            var headerBytes = System.Text.Encoding.UTF8.GetBytes(headerLine + "\r\n");
            var bytes = bom.Concat(headerBytes).ToArray();
            return Results.File(bytes, "text/csv", "contacts_import_template.csv");
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactRead)
        .WithName("GetContactImportTemplate")
        .WithDescription("Download the Google Contacts compatible CSV template. Supports direct import from Google Contacts exports.");

        // POST /api/contacts/import
        group.MapPost("/import", async (
            IFormFile file,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            using var stream = file.OpenReadStream();
            var command = new ImportContactsCommand { CsvStream = stream };
            var result = await mediator.Send(command, cancellationToken);
            var response = ApiResponse<ImportResult>.SuccessResult(result);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .DisableAntiforgery()
        .WithName("ImportContacts")
        .WithDescription("Import contacts from a CSV file (synchronous). Upserts by normalized phone then email. Use import-async for large files.");

        // POST /api/contacts/import-async
        group.MapPost("/import-async", async (
            IFormFile file,
            int? batchSize,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var dir = Path.Combine(Path.GetTempPath(), "ThaiX", "imports");
            Directory.CreateDirectory(dir);
            var filePath = Path.Combine(dir, Guid.NewGuid().ToString("N") + ".csv");
            await using (var dest = File.Create(filePath))
            await using (var src = file.OpenReadStream())
                await src.CopyToAsync(dest, cancellationToken);

            var jobId = await mediator.Send(new StartContactImportCommand { FilePath = filePath, BatchSize = batchSize }, cancellationToken);
            var response = ApiResponse<Guid>.SuccessResult(jobId);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Accepted($"/api/contacts/import/jobs/{jobId}", response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactWrite)
        .DisableAntiforgery()
        .WithName("StartContactImport")
        .WithDescription("Start asynchronous contact import. Returns job ID; poll GET /api/contacts/import/jobs/{id} for status.");

        // GET /api/contacts/import/jobs/{id}
        group.MapGet("/import/jobs/{id:guid}", async (
            Guid id,
            IMediator mediator,
            HttpContext httpContext,
            CancellationToken cancellationToken) =>
        {
            var job = await mediator.Send(new GetContactImportJobQuery { JobId = id }, cancellationToken);
            if (job is null)
                return Results.NotFound();
            var response = ApiResponse<ContactImportJobDto>.SuccessResult(job);
            response.Metadata.CorrelationId = httpContext.GetCorrelationId();
            return Results.Ok(response);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactRead)
        .WithName("GetContactImportJob")
        .WithDescription("Get status and result of an asynchronous contact import job.");

        // GET /api/contacts/export
        group.MapGet("/export", async (
            [AsParameters] ExportContactQueryParameters queryParams,
            IMediator mediator,
            CancellationToken cancellationToken) =>
        {
            var result = await mediator.Send(new ExportContactsQuery
            {
                SearchTerm = queryParams.SearchTerm,
                IsArchived = queryParams.IsArchived,
                Tag = queryParams.Tag
            }, cancellationToken);

            return Results.File(result.FileContent, "text/csv", result.FileName);
        })
        .RequireAuthorization(Domain.Common.Constants.Permissions.ContactRead)
        .WithName("ExportContacts")
        .WithDescription("Export contacts to a CSV file with optional filters");

        #endregion
    }

    private static string InferImageContentType(string fileName)
    {
        var ext = Path.GetExtension(fileName).TrimStart('.').ToLowerInvariant();
        return ext switch
        {
            "jpg" or "jpeg" => "image/jpeg",
            "png" => "image/png",
            "webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}

#region Query Parameter Models

public sealed record ContactQueryParameters
{
    [FromQuery(Name = "pageNumber")]
    [DefaultValue(1)]
    public int? PageNumber { get; init; }

    [FromQuery(Name = "pageSize")]
    [DefaultValue(10)]
    public int? PageSize { get; init; }

    [FromQuery(Name = "sortBy")]
    [DefaultValue("lastname")]
    public string? SortBy { get; init; }

    [FromQuery(Name = "sortDescending")]
    [DefaultValue(false)]
    public bool? SortDescending { get; init; }

    [FromQuery(Name = "search")]
    public string? SearchTerm { get; init; }

    [FromQuery(Name = "isArchived")]
    public bool? IsArchived { get; init; }

    [FromQuery(Name = "tag")]
    public string? Tag { get; init; }
}

public sealed record ExportContactQueryParameters
{
    [FromQuery(Name = "search")]
    public string? SearchTerm { get; init; }

    [FromQuery(Name = "isArchived")]
    public bool? IsArchived { get; init; }

    [FromQuery(Name = "tag")]
    public string? Tag { get; init; }
}

#endregion

#region Request Models

public sealed record UpdateContactProfileRequest
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public string? Company { get; init; }
    public string? JobTitle { get; init; }
    public string? AvatarUrl { get; init; }
    public DateOnly? Birthday { get; init; }
    public string? Notes { get; init; }
}

public sealed record ArchiveContactRequest
{
    public required bool Archive { get; init; }
}

public sealed record AddEmailRequest
{
    public required string Value { get; init; }
    public bool IsPrimary { get; init; }
}

public sealed record AddPhoneRequest
{
    public required string Value { get; init; }
    public bool IsPrimary { get; init; }
}

public sealed record AddAddressRequest
{
    public required string Street { get; init; }
    public required string CountryCode { get; init; }
    public string CityCode { get; init; } = string.Empty;
    public string DistrictCode { get; init; } = string.Empty;
    public string PostalCode { get; init; } = string.Empty;
    public bool IsPrimary { get; init; }
}

public sealed record AddSocialLinkRequest
{
    public required string Platform { get; init; }
    public required string Url { get; init; }
}

public sealed record AddTagRequest
{
    public required string Name { get; init; }
}

public sealed record AddBankAccountRequest
{
    public required string BankCode { get; init; }
    public string? BranchName { get; init; }
    public required string AccountNumber { get; init; }
    public required string AccountName { get; init; }
    public required string CurrencyCode { get; init; }
    public bool IsPrimary { get; init; }
}

public sealed record AddIdentityDocumentRequest
{
    public required string DocumentType { get; init; }
    public required string DocumentNumber { get; init; }
    public required string IssuedBy { get; init; }
    public required string IssuedPlace { get; init; }
    public required DateOnly IssuedDate { get; init; }
    public DateOnly? ExpiryDate { get; init; }
}

public sealed record SetCustomFieldRequest
{
    public required string Key { get; init; }
    public required string Value { get; init; }
}

#endregion
