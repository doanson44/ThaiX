namespace ThaiX.Presentation.IntegrationTests.Infrastructure;

/// <summary>
/// xUnit collection definition that ensures all integration tests sharing the same
/// <see cref="ThaiXWebApplicationFactory"/> run sequentially and share a single
/// SQL Server container lifecycle.
/// </summary>
[CollectionDefinition(Name)]
public sealed class IntegrationTestCollection : ICollectionFixture<ThaiXWebApplicationFactory>
{
    public const string Name = "Integration Tests";
}
