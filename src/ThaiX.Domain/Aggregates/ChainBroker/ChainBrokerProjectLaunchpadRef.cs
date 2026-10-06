namespace ThaiX.Domain.Aggregates.ChainBroker;

/// <summary>Reference to a launchpad that featured a ChainBroker project.</summary>
public sealed class ChainBrokerProjectLaunchpadRef
{
    private ChainBrokerProjectLaunchpadRef() { }

    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Slug { get; private set; } = string.Empty;
    public string? Name { get; private set; }

    internal static ChainBrokerProjectLaunchpadRef Create(Guid projectId, string slug, string? name) => new()
    {
        Id = Guid.NewGuid(),
        ProjectId = projectId,
        Slug = slug,
        Name = name
    };
}
