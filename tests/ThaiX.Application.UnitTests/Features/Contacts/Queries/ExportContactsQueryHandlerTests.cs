using ThaiX.Application.Common.Interfaces;
using ThaiX.Application.Features.Contacts.Queries.ExportContacts;
using ThaiX.Application.UnitTests.Common;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Application.UnitTests.Features.Contacts.Queries;

public sealed class ExportContactsQueryHandlerTests
{
    [Fact]
    public async Task Handle_WhenContactsExist_ReturnsCsvFileContent()
    {
        var contact = Contact.Create("Jane", "Doe", "Example Inc", "CEO", null, null, "Notes");
        contact.AddEmail("jane.doe@example.com", true);
        contact.AddPhone("+1234567890", true);
        contact.AddTag("Important");
        contact.AddAddress("123 Main St", "US", "NY", "NY", "10001", true);
        contact.AddSocialLink("LinkedIn", "https://linkedin.com/in/janedoe");
        contact.AddBankAccount("XYZ", "Main", "encrypted", "6789", "Jane Doe", "USD", true);

        var contacts = new[] { contact };
        var dbContext = new Mock<IApplicationDbContext>();
        dbContext.Setup(x => x.Contacts).Returns(TestDbSetHelpers.CreateMockDbSet(contacts).Object);

        var handler = new ExportContactsQueryHandler(dbContext.Object);
        var result = await handler.Handle(new ExportContactsQuery(), CancellationToken.None);

        result.FileContent.Should().NotBeNullOrEmpty();
        var csv = System.Text.Encoding.UTF8.GetString(result.FileContent);
        csv.Should().Contain("Jane");
        csv.Should().Contain("Doe");
        csv.Should().Contain("jane.doe@example.com");
        csv.Should().Contain("1234567890");
    }
}
