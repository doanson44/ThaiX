using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using ThaiX.Application.Common.Models;
using ThaiX.Application.Features.Contacts.Queries.GetContactById;
using ThaiX.Application.Features.Contacts.Queries.GetContacts;
using ThaiX.Domain.Common.Constants;
using ThaiX.Presentation.IntegrationTests.Infrastructure;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class ContactEndpointsTests : IntegrationTestBase
{
    public ContactEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetContacts_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/contacts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetContacts_WithoutContactReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"contact-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/contacts");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetContacts_WithContactReadPermission_ShouldReturnOk()
    {
        // Arrange
        var email = $"contact-reader-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.ContactRead });
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/contacts?pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetContacts_WithSearchTerm_ShouldReturnFilteredItems()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var aliceName = $"Alice{Guid.NewGuid():N}";
        var bobName = $"Bob{Guid.NewGuid():N}";
        await CreateContactAsync(aliceName, "Trader");
        await CreateContactAsync(bobName, "Analyst");

        // Act
        var response = await Client.GetAsync($"/api/contacts?search={aliceName}&pageNumber=1&pageSize=10");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadPagedResponseAsync<ContactListItemDto>(response);
        envelope.Data.Should().NotBeNull();
        envelope.Data!.Should().ContainSingle(x => x.FirstName == aliceName);
    }

    [Fact]
    public async Task GetContactById_WithUnknownId_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateContactReaderAsync();

        // Act
        var response = await Client.GetAsync($"/api/contacts/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateContact_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.PostAsJsonAsync("/api/contacts", new
        {
            FirstName = "Alice",
            LastName = "Trader"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task CreateContact_WithoutContactWritePermission_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateContactReaderAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/contacts", new
        {
            FirstName = "Alice",
            LastName = "Trader"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task CreateContact_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateContactWriterAsync();

        // Act
        var response = await Client.PostAsJsonAsync("/api/contacts", new
        {
            FirstName = string.Empty,
            LastName = "Trader"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task CreateContact_WithValidData_ShouldReturnCreated()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var firstName = $"Alice-{Guid.NewGuid():N}";

        // Act
        var contactId = await CreateContactAsync(firstName, "Trader", company: "ThaiX", jobTitle: "Investor");
        var detail = await GetContactAsync(contactId);

        // Assert
        detail.FirstName.Should().Be(firstName);
        detail.LastName.Should().Be("Trader");
        detail.Company.Should().Be("ThaiX");
        detail.JobTitle.Should().Be("Investor");
        detail.IsArchived.Should().BeFalse();
    }

    [Fact]
    public async Task UpdateContact_WithValidData_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Original", "Name");

        // Act
        var response = await Client.PutAsJsonAsync($"/api/contacts/{contactId}", new
        {
            FirstName = "Updated",
            LastName = "Contact",
            Company = "ThaiX Labs",
            JobTitle = "Lead Trader",
            AvatarUrl = "https://example.com/avatar.png",
            Birthday = new DateOnly(1990, 1, 2),
            Notes = "Updated by integration test"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var detail = await GetContactAsync(contactId);
        detail.FirstName.Should().Be("Updated");
        detail.LastName.Should().Be("Contact");
        detail.Company.Should().Be("ThaiX Labs");
        detail.JobTitle.Should().Be("Lead Trader");
        detail.AvatarUrl.Should().Be("https://example.com/avatar.png");
        detail.Birthday.Should().Be(new DateOnly(1990, 1, 2));
        detail.Notes.Should().Be("Updated by integration test");
    }

    [Fact]
    public async Task UpdateContact_WithUnknownId_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateContactWriterAsync();

        // Act
        var response = await Client.PutAsJsonAsync($"/api/contacts/{Guid.NewGuid()}", new
        {
            FirstName = "Updated",
            LastName = "Contact"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ArchiveContact_WithValidRequest_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Archive", "Me");

        // Act
        var response = await Client.PatchAsJsonAsync($"/api/contacts/{contactId}/archive", new
        {
            Archive = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await GetContactAsync(contactId);
        detail.IsArchived.Should().BeTrue();
    }

    [Fact]
    public async Task AddContactEmail_WithInvalidEmail_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateContactWriterAsync();
        var contactId = await CreateContactAsync("Email", "Validation");

        // Act
        var response = await Client.PostAsJsonAsync($"/api/contacts/{contactId}/emails", new
        {
            Value = "not-an-email",
            IsPrimary = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddContactEmail_WithValidData_ShouldPersistEmail()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Email", "Primary");

        // Act
        var response = await Client.PostAsJsonAsync($"/api/contacts/{contactId}/emails", new
        {
            Value = "primary@thaix.test",
            IsPrimary = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await GetContactAsync(contactId);
        detail.Emails.Should().ContainSingle(x => x.Value == "primary@thaix.test" && x.IsPrimary);
    }

    [Fact]
    public async Task SetContactEmailPrimary_WithSecondEmail_ShouldSwitchPrimary()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Email", "Switch");
        await Client.PostAsJsonAsync($"/api/contacts/{contactId}/emails", new { Value = "first@thaix.test", IsPrimary = true });
        await Client.PostAsJsonAsync($"/api/contacts/{contactId}/emails", new { Value = "second@thaix.test", IsPrimary = false });
        var before = await GetContactAsync(contactId);
        var secondEmailId = before.Emails.Single(x => x.Value == "second@thaix.test").Id;

        // Act
        var response = await Client.PatchAsync($"/api/contacts/{contactId}/emails/{secondEmailId}/primary", content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await GetContactAsync(contactId);
        detail.Emails.Should().ContainSingle(x => x.Value == "second@thaix.test" && x.IsPrimary);
        detail.Emails.Should().ContainSingle(x => x.Value == "first@thaix.test" && !x.IsPrimary);
    }

    [Fact]
    public async Task RemoveContactEmail_WithValidId_ShouldRemoveEmail()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Email", "Remove");
        await Client.PostAsJsonAsync($"/api/contacts/{contactId}/emails", new { Value = "remove@thaix.test", IsPrimary = true });
        var before = await GetContactAsync(contactId);
        var emailId = before.Emails.Single(x => x.Value == "remove@thaix.test").Id;

        // Act
        var response = await Client.DeleteAsync($"/api/contacts/{contactId}/emails/{emailId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await GetContactAsync(contactId);
        detail.Emails.Should().NotContain(x => x.Id == emailId);
    }

    [Fact]
    public async Task AddContactPhone_WithInvalidRequest_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateContactWriterAsync();
        var contactId = await CreateContactAsync("Phone", "Validation");

        // Act
        var response = await Client.PostAsJsonAsync($"/api/contacts/{contactId}/phones", new
        {
            Value = string.Empty,
            IsPrimary = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddContactPhone_WithValidData_ShouldPersistPhone()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Phone", "Primary");
        var phoneNumber = GenerateUniquePhoneNumber();

        // Act
        var response = await Client.PostAsJsonAsync($"/api/contacts/{contactId}/phones", new
        {
            Value = phoneNumber,
            IsPrimary = true
        });
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, responseBody);
        var detail = await GetContactAsync(contactId);
        detail.Phones.Should().ContainSingle(x => x.Value == phoneNumber && x.IsPrimary);
    }

    [Fact]
    public async Task SetContactPhonePrimary_WithSecondPhone_ShouldSwitchPrimary()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Phone", "Switch");
        var firstPhone = GenerateUniquePhoneNumber();
        var secondPhone = GenerateUniquePhoneNumber();
        var firstResponse = await Client.PostAsJsonAsync($"/api/contacts/{contactId}/phones", new { Value = firstPhone, IsPrimary = true });
        var firstResponseBody = await firstResponse.Content.ReadAsStringAsync();
        firstResponse.StatusCode.Should().Be(HttpStatusCode.OK, firstResponseBody);
        var secondResponse = await Client.PostAsJsonAsync($"/api/contacts/{contactId}/phones", new { Value = secondPhone, IsPrimary = false });
        var secondResponseBody = await secondResponse.Content.ReadAsStringAsync();
        secondResponse.StatusCode.Should().Be(HttpStatusCode.OK, secondResponseBody);
        var before = await GetContactAsync(contactId);
        var secondPhoneId = before.Phones.Single(x => x.Value == secondPhone).Id;

        // Act
        var response = await Client.PatchAsync($"/api/contacts/{contactId}/phones/{secondPhoneId}/primary", content: null);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await GetContactAsync(contactId);
        detail.Phones.Should().ContainSingle(x => x.Value == secondPhone && x.IsPrimary);
        detail.Phones.Should().ContainSingle(x => x.Value == firstPhone && !x.IsPrimary);
    }

    [Fact]
    public async Task RemoveContactPhone_WithValidId_ShouldRemovePhone()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Phone", "Remove");
        var phoneNumber = GenerateUniquePhoneNumber();
        var createPhoneResponse = await Client.PostAsJsonAsync($"/api/contacts/{contactId}/phones", new { Value = phoneNumber, IsPrimary = true });
        var createPhoneResponseBody = await createPhoneResponse.Content.ReadAsStringAsync();
        createPhoneResponse.StatusCode.Should().Be(HttpStatusCode.OK, createPhoneResponseBody);
        var before = await GetContactAsync(contactId);
        var phoneId = before.Phones.Single(x => x.Value == phoneNumber).Id;

        // Act
        var response = await Client.DeleteAsync($"/api/contacts/{contactId}/phones/{phoneId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await GetContactAsync(contactId);
        detail.Phones.Should().NotContain(x => x.Id == phoneId);
    }

    [Fact]
    public async Task DeleteContact_WithoutContactDeletePermission_ShouldReturnForbidden()
    {
        // Arrange
        await AuthenticateContactWriterAsync();
        var contactId = await CreateContactAsync("Delete", "Forbidden");

        // Act
        var response = await Client.DeleteAsync($"/api/contacts/{contactId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task DeleteContact_WithContactDeletePermission_ShouldSoftDeleteContact()
    {
        // Arrange
        var email = $"contact-delete-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.ContactDelete, Permissions.ContactRead, Permissions.ContactWrite });
        await AuthenticateAsync(email, "Test@Pass123");
        var contactId = await CreateContactAsync("Delete", "Me");

        // Act
        var response = await Client.DeleteAsync($"/api/contacts/{contactId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var getResponse = await Client.GetAsync($"/api/contacts/{contactId}");
        getResponse.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddContactAddress_WithValidData_ShouldPersistAddress()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Address", "Create");

        // Act
        var response = await Client.PostAsJsonAsync($"/api/contacts/{contactId}/addresses", new
        {
            Street = "123 Main Street",
            CountryCode = "VN",
            CityCode = "HCM",
            DistrictCode = "D1",
            PostalCode = "700000",
            IsPrimary = true
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await GetContactAsync(contactId);
        detail.Addresses.Should().ContainSingle(x =>
            x.Street == "123 Main Street" &&
            x.CountryCode == "VN" &&
            x.CityCode == "HCM" &&
            x.DistrictCode == "D1" &&
            x.PostalCode == "700000" &&
            x.IsPrimary);
    }

    [Fact]
    public async Task RemoveContactAddress_WithValidId_ShouldRemoveAddress()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Address", "Remove");
        await Client.PostAsJsonAsync($"/api/contacts/{contactId}/addresses", new
        {
            Street = "456 Remove Street",
            CountryCode = "VN",
            CityCode = "HN",
            DistrictCode = "HK",
            PostalCode = "100000",
            IsPrimary = true
        });
        var before = await GetContactAsync(contactId);
        var addressId = before.Addresses.Single().Id;

        // Act
        var response = await Client.DeleteAsync($"/api/contacts/{contactId}/addresses/{addressId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await GetContactAsync(contactId);
        detail.Addresses.Should().BeEmpty();
    }

    [Fact]
    public async Task AddContactSocialLink_WithValidData_ShouldPersistSocialLink()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Social", "Create");

        // Act
        var response = await Client.PostAsJsonAsync($"/api/contacts/{contactId}/social-links", new
        {
            Platform = "LinkedIn",
            Url = "https://linkedin.com/in/test-user"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await GetContactAsync(contactId);
        detail.SocialLinks.Should().ContainSingle(x => x.Platform == "LinkedIn" && x.Url == "https://linkedin.com/in/test-user");
    }

    [Fact]
    public async Task RemoveContactSocialLink_WithValidId_ShouldRemoveSocialLink()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Social", "Remove");
        await Client.PostAsJsonAsync($"/api/contacts/{contactId}/social-links", new
        {
            Platform = "X",
            Url = "https://x.com/test-user"
        });
        var before = await GetContactAsync(contactId);
        var socialLinkId = before.SocialLinks.Single().Id;

        // Act
        var response = await Client.DeleteAsync($"/api/contacts/{contactId}/social-links/{socialLinkId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await GetContactAsync(contactId);
        detail.SocialLinks.Should().BeEmpty();
    }

    [Fact]
    public async Task AddContactTag_WithValidData_ShouldPersistTag()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Tag", "Create");

        // Act
        var response = await Client.PostAsJsonAsync($"/api/contacts/{contactId}/tags", new
        {
            Name = "vip-client"
        });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await GetContactAsync(contactId);
        detail.Tags.Should().ContainSingle(x => x.Name == "vip-client");
    }

    [Fact]
    public async Task AddContactTag_WithDuplicateName_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateContactWriterAsync();
        var contactId = await CreateContactAsync("Tag", "Duplicate");
        await Client.PostAsJsonAsync($"/api/contacts/{contactId}/tags", new { Name = "vip-client" });

        // Act
        var response = await Client.PostAsJsonAsync($"/api/contacts/{contactId}/tags", new { Name = "VIP-CLIENT" });

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task RemoveContactTag_WithValidId_ShouldRemoveTag()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Tag", "Remove");
        await Client.PostAsJsonAsync($"/api/contacts/{contactId}/tags", new { Name = "remove-me" });
        var before = await GetContactAsync(contactId);
        var tagId = before.Tags.Single().Id;

        // Act
        var response = await Client.DeleteAsync($"/api/contacts/{contactId}/tags/{tagId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await GetContactAsync(contactId);
        detail.Tags.Should().BeEmpty();
    }

    [Fact]
    public async Task AddContactBankAccount_WithValidData_ShouldPersistMaskedAccount()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Bank", "Create");

        // Act
        var response = await Client.PostAsJsonAsync($"/api/contacts/{contactId}/bank-accounts", new
        {
            BankCode = "VCB",
            BranchName = "HCM",
            AccountNumber = "123456789012",
            AccountName = "Test User",
            CurrencyCode = "VND",
            IsPrimary = true
        });
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, responseBody);
        var detail = await GetContactAsync(contactId);
        detail.BankAccounts.Should().ContainSingle(x =>
            x.BankCode == "VCB" &&
            x.AccountName == "Test User" &&
            x.CurrencyCode == "VND" &&
            x.AccountNumberLast4 == "9012" &&
            x.IsPrimary);
    }

    [Fact]
    public async Task RemoveContactBankAccount_WithValidId_ShouldRemoveBankAccount()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Bank", "Remove");
        var addResponse = await Client.PostAsJsonAsync($"/api/contacts/{contactId}/bank-accounts", new
        {
            BankCode = "ACB",
            BranchName = "HN",
            AccountNumber = "987654321000",
            AccountName = "Remove User",
            CurrencyCode = "USD",
            IsPrimary = true
        });
        var addResponseBody = await addResponse.Content.ReadAsStringAsync();
        addResponse.StatusCode.Should().Be(HttpStatusCode.OK, addResponseBody);
        var before = await GetContactAsync(contactId);
        var bankAccountId = before.BankAccounts.Single().Id;

        // Act
        var response = await Client.DeleteAsync($"/api/contacts/{contactId}/bank-accounts/{bankAccountId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await GetContactAsync(contactId);
        detail.BankAccounts.Should().BeEmpty();
    }

    [Fact]
    public async Task AddContactIdentityDocument_WithValidData_ShouldPersistDocument()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Identity", "Create");

        // Act
        var response = await Client.PostAsJsonAsync($"/api/contacts/{contactId}/identity-documents", new
        {
            DocumentType = "NationalId",
            DocumentNumber = "012345678901",
            IssuedBy = "CA",
            IssuedPlace = "HCM",
            IssuedDate = new DateOnly(2020, 1, 1),
            ExpiryDate = new DateOnly(2030, 1, 1)
        });
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, responseBody);
        var detail = await GetContactAsync(contactId);
        detail.IdentityDocuments.Should().ContainSingle(x =>
            x.DocumentType == "NationalId" &&
            x.DocumentNumberLast4 == "8901" &&
            x.IssuedBy == "CA" &&
            x.IssuedPlace == "HCM");
    }

    [Fact]
    public async Task RemoveContactIdentityDocument_WithValidId_ShouldRemoveDocument()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Identity", "Remove");
        var addResponse = await Client.PostAsJsonAsync($"/api/contacts/{contactId}/identity-documents", new
        {
            DocumentType = "Passport",
            DocumentNumber = "A123456789",
            IssuedBy = "Immigration",
            IssuedPlace = "VN",
            IssuedDate = new DateOnly(2021, 5, 10),
            ExpiryDate = new DateOnly(2031, 5, 10)
        });
        var addResponseBody = await addResponse.Content.ReadAsStringAsync();
        addResponse.StatusCode.Should().Be(HttpStatusCode.OK, addResponseBody);
        var before = await GetContactAsync(contactId);
        var identityDocumentId = before.IdentityDocuments.Single().Id;

        // Act
        var response = await Client.DeleteAsync($"/api/contacts/{contactId}/identity-documents/{identityDocumentId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var detail = await GetContactAsync(contactId);
        detail.IdentityDocuments.Should().BeEmpty();
    }

    [Fact]
    public async Task SetAndRemoveCustomField_WithValidData_ShouldUpdateContact()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Custom", "Field");

        // Act
        var setResponse = await Client.PutAsJsonAsync($"/api/contacts/{contactId}/custom-fields", new
        {
            Key = "riskProfile",
            Value = "Aggressive"
        });

        // Assert
        setResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        var removeResponse = await Client.DeleteAsync($"/api/contacts/{contactId}/custom-fields/riskProfile");

        // Assert
        removeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UploadAndRemoveAvatar_WithValidImage_ShouldUpdateAvatarUrl()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Avatar", "User");
        using var uploadContent = CreateFileFormData("file", "avatar.png", "image/png", "fake-image-content");

        // Act
        var uploadResponse = await Client.PostAsync($"/api/contacts/{contactId}/avatar", uploadContent);

        // Assert
        uploadResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var uploadEnvelope = await ReadResponseAsync<string>(uploadResponse);
        uploadEnvelope.Data.Should().NotBeNullOrWhiteSpace();
        var detailAfterUpload = await GetContactAsync(contactId);
        detailAfterUpload.AvatarUrl.Should().NotBeNullOrWhiteSpace();

        // Act
        var removeResponse = await Client.DeleteAsync($"/api/contacts/{contactId}/avatar");

        // Assert
        removeResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        var detailAfterRemove = await GetContactAsync(contactId);
        detailAfterRemove.AvatarUrl.Should().BeNull();
    }

    [Fact]
    public async Task SuggestUsers_WithContactReadPermissionAndNoPhone_ShouldReturnEmptyList()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var contactId = await CreateContactAsync("Suggest", "Users");

        // Act
        var response = await Client.GetAsync($"/api/contacts/{contactId}/suggest-users");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var envelope = await ReadResponseAsync<IReadOnlyList<object>>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data.Should().BeEmpty();
    }

    [Fact]
    public async Task GetContactImportTemplate_WithContactReadPermission_ShouldReturnCsvTemplate()
    {
        // Arrange
        await AuthenticateContactReaderAsync();

        // Act
        var response = await Client.GetAsync("/api/contacts/import/template");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        var body = await response.Content.ReadAsStringAsync();
        // Template uses Google Contacts format
        body.Should().Contain("Given Name");
        body.Should().Contain("Family Name");
        body.Should().Contain("E-mail 1 - Value");
        body.Should().Contain("Phone 1 - Value");
    }

    [Fact]
    public async Task ExportContacts_WithContactReadPermission_ShouldReturnCsvFile()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var firstName = $"Export{Guid.NewGuid():N}";
        await CreateContactAsync(firstName, "Contact");

        // Act
        var response = await Client.GetAsync($"/api/contacts/export?search={firstName}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType.Should().NotBeNull();
        response.Content.Headers.ContentType!.MediaType.Should().Be("text/csv");
        var body = await response.Content.ReadAsStringAsync();
        // Export uses Google Contacts format
        body.Should().Contain("Given Name");
        body.Should().Contain("Family Name");
        body.Should().Contain(firstName);
    }

    [Fact]
    public async Task StartContactImport_WithValidCsv_ShouldReturnAccepted()
    {
        // Arrange
        await AuthenticateContactWriterAsync();
        var csv = "FirstName,LastName,Email1\nImport,User,import.user@thaix.test\n";
        using var importContent = CreateFileFormData("file", "contacts.csv", "text/csv", csv);

        // Act
        var response = await Client.PostAsync("/api/contacts/import-async?batchSize=10", importContent);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Accepted);
        var envelope = await ReadResponseAsync<Guid>(response);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public async Task GetContactImportJob_WithUnknownId_ShouldReturnNotFound()
    {
        // Arrange
        await AuthenticateContactReaderAsync();

        // Act
        var response = await Client.GetAsync($"/api/contacts/import/jobs/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task ImportContacts_WithValidCsv_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateContactWriterAsync();
        var csv = "FirstName,LastName,Email1\nSync,Import,sync.import@thaix.test\n";
        using var importContent = CreateFileFormData("file", "contacts-sync.csv", "text/csv", csv);

        // Act
        var response = await Client.PostAsync("/api/contacts/import", importContent);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, responseBody);
        responseBody.Should().Contain("insertedCount");
    }

    // ---- template 401/403 ----

    [Fact]
    public async Task GetContactImportTemplate_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/contacts/import/template");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetContactImportTemplate_WithoutContactReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"contact-tmpl-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/contacts/import/template");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ---- export 401/403 ----

    [Fact]
    public async Task ExportContacts_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/contacts/export");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ExportContacts_WithoutContactReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"contact-export-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync("/api/contacts/export");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ExportContacts_WithSearchFilter_ShouldReturnOnlyMatchingRows()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var targetFirst = $"FilterTarget{Guid.NewGuid():N}";
        await CreateContactAsync(targetFirst, "Exported");
        await CreateContactAsync("NoMatch", "OtherUser");

        // Act
        var response = await Client.GetAsync($"/api/contacts/export?search={targetFirst}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain(targetFirst);
        body.Should().NotContain("NoMatch");
    }

    [Fact]
    public async Task ExportContacts_WithAllRelatedData_ShouldIncludeAllColumns()
    {
        // Arrange
        await AuthenticateContactReaderWriterAsync();
        var first = $"FullExport{Guid.NewGuid():N}";
        var contactId = await CreateContactAsync(first, "AllCols", "TestCorp", "Engineer");
        await Client.PostAsJsonAsync($"/api/contacts/{contactId}/emails", new { Value = $"{first}@test.com", IsPrimary = true });
        await Client.PostAsJsonAsync($"/api/contacts/{contactId}/phones", new { Value = "+84900000001", IsPrimary = true });

        // Act
        var response = await Client.GetAsync($"/api/contacts/export?search={first}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var body = await response.Content.ReadAsStringAsync();
        body.Should().Contain("Organization 1 - Name");
        body.Should().Contain("E-mail 1 - Value");
        body.Should().Contain("Phone 1 - Value");
        body.Should().Contain("TestCorp");
        body.Should().Contain($"{first.ToLower()}@test.com");
    }

    // ---- import (sync) 401/403/invalid ----

    [Fact]
    public async Task ImportContacts_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var csv = "FirstName,LastName\nTest,User\n";
        using var content = CreateFileFormData("file", "contacts.csv", "text/csv", csv);

        // Act
        var response = await Client.PostAsync("/api/contacts/import", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task ImportContacts_WithoutContactWritePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"contact-import-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");
        var csv = "FirstName,LastName\nTest,User\n";
        using var content = CreateFileFormData("file", "contacts.csv", "text/csv", csv);

        // Act
        var response = await Client.PostAsync("/api/contacts/import", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task ImportContacts_WithInvalidCsvHeaders_ShouldReturnBadRequest()
    {
        // Arrange
        await AuthenticateContactWriterAsync();
        var csv = "Column1,Column2\nValue1,Value2\n";
        using var content = CreateFileFormData("file", "bad.csv", "text/csv", csv);

        // Act
        var response = await Client.PostAsync("/api/contacts/import", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task ImportContacts_WithGoogleContactsFormat_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateContactWriterAsync();
        var email = $"google-import-{Guid.NewGuid():N}@thaix.test";
        var csv = $"Given Name,Family Name,E-mail 1 - Value,Phone 1 - Value,Organization 1 - Name\nGoogleFirst,GoogleLast,{email},+84900000099,GoogleCorp\n";
        using var content = CreateFileFormData("file", "google-contacts.csv", "text/csv", csv);

        // Act
        var response = await Client.PostAsync("/api/contacts/import", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, responseBody);
        responseBody.Should().Contain("insertedCount");
    }

    [Fact]
    public async Task ImportContacts_WithCombinedFixtureCsv_ShouldImportAllRows()
    {
        // Arrange
        await AuthenticateContactWriterAsync();
        var fixturePath = GetContactsFixturePath("contacts_import_combined.csv");
        File.Exists(fixturePath).Should().BeTrue($"Fixture not found at {fixturePath}");
        var expectedDataRows = CountCsvDataRows(fixturePath);
        using var content = CreateFileFormDataFromPath("file", "contacts_import_combined.csv", "text/csv", fixturePath);

        // Act
        var response = await Client.PostAsync("/api/contacts/import", content);
        var envelope = await ReadResponseAsync<ImportResult>(response);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.TotalRows.Should().Be(expectedDataRows);
        envelope.Data.SkippedCount.Should().Be(0, string.Join("; ", envelope.Data.Errors.Select(e => $"Row {e.RowNumber}: {e.Error}")));
        envelope.Data.InsertedCount.Should().BeGreaterThan(0);
        (envelope.Data.InsertedCount + envelope.Data.UpdatedCount).Should().Be(expectedDataRows);
    }

    [Fact]
    public async Task ImportContacts_WithCombinedFixtureCsv_Reimport_ShouldUpdateNotDuplicate()
    {
        // Arrange
        await AuthenticateContactWriterAsync();
        var fixturePath = GetContactsFixturePath("contacts_import_combined.csv");
        var expectedDataRows = CountCsvDataRows(fixturePath);

        using var content1 = CreateFileFormDataFromPath("file", "contacts_import_combined.csv", "text/csv", fixturePath);
        var response1 = await Client.PostAsync("/api/contacts/import", content1);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        // Act
        using var content2 = CreateFileFormDataFromPath("file", "contacts_import_combined.csv", "text/csv", fixturePath);
        var response2 = await Client.PostAsync("/api/contacts/import", content2);
        var envelope = await ReadResponseAsync<ImportResult>(response2);

        // Assert
        response2.StatusCode.Should().Be(HttpStatusCode.OK);
        envelope.Success.Should().BeTrue();
        envelope.Data.Should().NotBeNull();
        envelope.Data!.TotalRows.Should().Be(expectedDataRows);
        envelope.Data.SkippedCount.Should().Be(0);
        envelope.Data.InsertedCount.Should().Be(0);
        envelope.Data.UpdatedCount.Should().Be(expectedDataRows);
    }

    [Fact]
    public async Task ImportContacts_BulkImportOver500Contacts_ShouldReturnOk()
    {
        // Arrange
        await AuthenticateContactWriterAsync();
        var csvBuilder = new StringBuilder();
        csvBuilder.AppendLine("Given Name,Family Name,E-mail 1 - Value,Phone 1 - Value,Organization 1 - Name,Organization 1 - Title");
        for (int i = 1; i <= 501; i++)
        {
            csvBuilder.AppendLine($"Contact{i},Last{i},contact{i}@test.com,+84900000{i:D3},TestCorp,Position{i}");
        }
        var csv = csvBuilder.ToString();
        using var content = CreateFileFormData("file", "bulk_contacts.csv", "text/csv", csv);

        // Act
        var response = await Client.PostAsync("/api/contacts/import", content);
        var responseBody = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK, responseBody);
        responseBody.Should().Contain("insertedCount");
        responseBody.Should().Contain("\"insertedCount\":501");
    }

    // ---- import (sync) concurrency/duplicate scenarios ----

    [Fact]
    public async Task ImportContacts_ReimportSameCsv_ShouldUpdateNotInsertDuplicate()
    {
        // Arrange
        await AuthenticateContactWriterAsync();
        var email = $"reimport-{Guid.NewGuid():N}@thaix.test";
        var csv = $"Given Name,Family Name,E-mail 1 - Value,Phone 1 - Value,Organization 1 - Name\nReimportFirst,ReimportLast,{email},0987654321,TestCorp\n";

        // Act: first import
        using var content1 = CreateFileFormData("file", "contacts.csv", "text/csv", csv);
        var response1 = await Client.PostAsync("/api/contacts/import", content1);
        var body1 = await response1.Content.ReadAsStringAsync();

        // Assert: first import inserts
        response1.StatusCode.Should().Be(HttpStatusCode.OK, body1);
        body1.Should().Contain("\"insertedCount\":1");
        body1.Should().Contain("\"updatedCount\":0");

        // Act: re-import same CSV
        using var content2 = CreateFileFormData("file", "contacts.csv", "text/csv", csv);
        var response2 = await Client.PostAsync("/api/contacts/import", content2);
        var body2 = await response2.Content.ReadAsStringAsync();

        // Assert: re-import updates existing (no duplicate)
        response2.StatusCode.Should().Be(HttpStatusCode.OK, body2);
        body2.Should().Contain("\"insertedCount\":0");
        body2.Should().Contain("\"updatedCount\":1");
        body2.Should().Contain("\"skippedCount\":0");
    }

    [Fact]
    public async Task ImportContacts_WithDuplicatePhoneAcrossDifferentContactsInSameCsv_ShouldNotCrash()
    {
        // Arrange
        await AuthenticateContactWriterAsync();
        var phone = GenerateUniquePhoneNumber();
        var email1 = $"dup-1-{Guid.NewGuid():N}@thaix.test";
        var email2 = $"dup-2-{Guid.NewGuid():N}@thaix.test";
        var csv = $"Given Name,Family Name,E-mail 1 - Value,Phone 1 - Value\nFirstContact,One,{email1},{phone}\nSecondContact,Two,{email2},{phone}\n";

        // Act
        using var content = CreateFileFormData("file", "contacts.csv", "text/csv", csv);
        var response = await Client.PostAsync("/api/contacts/import", content);
        var body = await response.Content.ReadAsStringAsync();

        // Assert: does not crash with duplicate key error (409 Conflict = ConcurrencyException)
        response.StatusCode.Should().Be(HttpStatusCode.OK, body);
        body.Should().Contain("\"skippedCount\":0");
    }

    // ---- import (sync) concurrency/duplicate scenarios ----

    [Fact]
    public async Task ImportContacts_SoftDeletedContactPhone_ShouldBeReusable()
    {
        // Arrange
        var uniqueId = Guid.NewGuid().ToString("N");
        var email = $"softdel-{uniqueId}@thaix.test";
        var phone = GenerateUniquePhoneNumber();
        var csv = $"Given Name,Family Name,E-mail 1 - Value,Phone 1 - Value\nSoftDelFirst,SoftDelLast,{email},{phone}\n";

        // Step 1: import contact
        await AuthenticateContactWriterAsync();
        using var content1 = CreateFileFormData("file", "contacts.csv", "text/csv", csv);
        var response1 = await Client.PostAsync("/api/contacts/import", content1);
        response1.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 2: soft-delete the contact
        await AuthenticateContactDeleterAsync();
        var listResponse = await Client.GetAsync("/api/contacts?search=SoftDelFirst&pageSize=5");
        var paged = await ReadPagedResponseAsync<ContactListItemDto>(listResponse);
        var contactId = paged.Data!.First().Id;

        var deleteResponse = await Client.DeleteAsync($"/api/contacts/{contactId}");
        deleteResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Step 3: re-import same CSV (phone should be reusable after soft-delete)
        await AuthenticateContactWriterAsync();
        using var content2 = CreateFileFormData("file", "contacts.csv", "text/csv", csv);
        var response2 = await Client.PostAsync("/api/contacts/import", content2);
        var body2 = await response2.Content.ReadAsStringAsync();

        // Assert: soft-deleted contact is restored and updated (by design)
        response2.StatusCode.Should().Be(HttpStatusCode.OK, body2);
        body2.Should().Contain("\"insertedCount\":0");
        body2.Should().Contain("\"updatedCount\":1");
    }

    // ---- import-async 401/403 ----

    [Fact]
    public async Task StartContactImport_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;
        var csv = "FirstName,LastName\nTest,User\n";
        using var content = CreateFileFormData("file", "contacts.csv", "text/csv", csv);

        // Act
        var response = await Client.PostAsync("/api/contacts/import-async", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task StartContactImport_WithoutContactWritePermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"contact-async-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");
        var csv = "FirstName,LastName\nTest,User\n";
        using var content = CreateFileFormData("file", "contacts.csv", "text/csv", csv);

        // Act
        var response = await Client.PostAsync("/api/contacts/import-async", content);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    // ---- import/jobs 401/403 ----

    [Fact]
    public async Task GetContactImportJob_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync($"/api/contacts/import/jobs/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetContactImportJob_WithoutContactReadPermission_ShouldReturnForbidden()
    {
        // Arrange
        var email = $"contact-job-noperm-{Guid.NewGuid():N}@thaix.test";
        await CreateTestUserAsync(email, "Test@Pass123", confirmEmail: true);
        await AuthenticateAsync(email, "Test@Pass123");

        // Act
        var response = await Client.GetAsync($"/api/contacts/import/jobs/{Guid.NewGuid()}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    private async Task AuthenticateContactReaderAsync()
    {
        var email = $"contact-reader-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.ContactRead });
        await AuthenticateAsync(email, "Test@Pass123");
    }

    private async Task AuthenticateContactDeleterAsync()
    {
        var email = $"contact-deleter-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.ContactDelete, Permissions.ContactRead, Permissions.ContactWrite });
        await AuthenticateAsync(email, "Test@Pass123");
    }

    private async Task AuthenticateContactWriterAsync()
    {
        var email = $"contact-writer-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.ContactWrite });
        await AuthenticateAsync(email, "Test@Pass123");
    }

    private async Task AuthenticateContactReaderWriterAsync()
    {
        var email = $"contact-rw-{Guid.NewGuid():N}@thaix.test";
        await CreateUserWithPermissionsAsync(email, "Test@Pass123", new[] { Permissions.ContactRead, Permissions.ContactWrite });
        await AuthenticateAsync(email, "Test@Pass123");
    }

    private async Task<Guid> CreateContactAsync(
        string firstName,
        string lastName,
        string? company = null,
        string? jobTitle = null)
    {
        var response = await Client.PostAsJsonAsync("/api/contacts", new
        {
            FirstName = firstName,
            LastName = lastName,
            Company = company,
            JobTitle = jobTitle
        });

        response.EnsureSuccessStatusCode();
        var envelope = await ReadResponseAsync<Guid>(response);
        return envelope.Data;
    }

    private async Task<ContactDetailDto> GetContactAsync(Guid contactId)
    {
        var response = await Client.GetAsync($"/api/contacts/{contactId}");
        response.EnsureSuccessStatusCode();
        var envelope = await ReadResponseAsync<ContactDetailDto>(response);
        return envelope.Data!;
    }

    private static async Task<PagedApiResponse<T>> ReadPagedResponseAsync<T>(HttpResponseMessage response)
    {
        var envelope = await response.Content.ReadFromJsonAsync<PagedApiResponse<T>>(JsonOptions);
        if (envelope is null)
        {
            throw new InvalidOperationException("Unable to deserialize paged API response.");
        }

        return envelope;
    }

    private static string GenerateUniquePhoneNumber()
    {
        var digits = DateTime.UtcNow.Ticks.ToString()[^9..];
        return $"849{digits}";
    }

    private static MultipartFormDataContent CreateFileFormData(string fieldName, string fileName, string contentType, string content)
    {
        var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(Encoding.UTF8.GetBytes(content));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, fieldName, fileName);
        return form;
    }

    private static MultipartFormDataContent CreateFileFormDataFromPath(string fieldName, string fileName, string contentType, string filePath)
    {
        var form = new MultipartFormDataContent();
        var fileContent = new ByteArrayContent(File.ReadAllBytes(filePath));
        fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
        form.Add(fileContent, fieldName, fileName);
        return form;
    }

    private static string GetContactsFixturePath(string fileName) =>
        Path.Combine(AppContext.BaseDirectory, "Fixtures", "Contacts", fileName);

    private static int CountCsvDataRows(string filePath) =>
        File.ReadLines(filePath).Count(line => !string.IsNullOrWhiteSpace(line)) - 1;
}
