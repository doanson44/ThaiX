using System.Net;
using System.Text.Json;
using FluentAssertions;
using ThaiX.Presentation.IntegrationTests.Infrastructure;

namespace ThaiX.Presentation.IntegrationTests.Endpoints;

[Collection(IntegrationTestCollection.Name)]
public sealed class PublicConfigurationEndpointsTests : IntegrationTestBase
{
    public PublicConfigurationEndpointsTests(ThaiXWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task GetDonateConfiguration_WithoutAuthentication_ShouldReturnPublicSettings()
    {
        // Arrange
        Client.DefaultRequestHeaders.Authorization = null;

        // Act
        var response = await Client.GetAsync("/api/public/donate-config");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        var root = document.RootElement;
        root.GetProperty("success").GetBoolean().Should().BeTrue();

        var data = root.GetProperty("data");
        data.GetProperty("enabled").GetBoolean().Should().BeTrue();
        data.GetProperty("qrImage").GetString().Should().Be("images/qr-donate.png");
        data.GetProperty("accountName").GetString().Should().NotContain("REPLACE_WITH");
        data.GetProperty("accountNumber").GetString().Should().NotContain("REPLACE_WITH");
    }
}
