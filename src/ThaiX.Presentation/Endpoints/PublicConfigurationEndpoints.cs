using Microsoft.Extensions.Configuration;
using ThaiX.Presentation.Models;

namespace ThaiX.Presentation.Endpoints;

public static class PublicConfigurationEndpoints
{
    public static IEndpointRouteBuilder MapPublicConfigurationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/api/public/donate-config", (IConfiguration configuration) =>
        {
            var donateConfiguration = new DonateConfigurationDto(
                configuration.GetValue("Donate:Enabled", true),
                GetPublicValue(configuration["Donate:QrImage"], "images/qr-donate.png"),
                GetPublicValue(configuration["Donate:AccountName"]),
                GetPublicValue(configuration["Donate:AccountNumber"]));

            return Results.Ok(ApiResponse<DonateConfigurationDto>.SuccessResult(donateConfiguration));
        })
        .AllowAnonymous()
        .WithName("GetPublicDonateConfiguration");

        return endpoints;
    }

    private static string GetPublicValue(string? value, string fallback = "")
    {
        return string.IsNullOrWhiteSpace(value)
            || value.Contains("REPLACE_WITH", StringComparison.OrdinalIgnoreCase)
                ? fallback
                : value;
    }
}
