namespace ThaiX.Presentation.Endpoints;

public static class HealthEndpoints
{
    public static void MapHealthEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapHealthChecks("/health")
            .AllowAnonymous()
            .WithName("HealthCheck");
    }
}
