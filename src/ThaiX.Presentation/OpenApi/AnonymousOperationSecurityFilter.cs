using Microsoft.AspNetCore.Authorization;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace ThaiX.Presentation.OpenApi;

/// <summary>
/// Clears global Bearer security for operations marked with AllowAnonymous.
/// </summary>
public sealed class AnonymousOperationSecurityFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var endpointMetadata = context.ApiDescription.ActionDescriptor.EndpointMetadata;
        if (endpointMetadata is null || endpointMetadata.Count == 0)
        {
            return;
        }

        var allowsAnonymous = endpointMetadata.OfType<IAllowAnonymous>().Any();
        if (!allowsAnonymous)
        {
            return;
        }

        operation.Security = new List<OpenApiSecurityRequirement>();
    }
}
