namespace ThaiX.Domain.Aggregates.ChainBroker;

/// <summary>Tag applied to a ChainBroker project (e.g. "DeFi", "Layer 1").</summary>
public sealed class ChainBrokerProjectTag
{
    private ChainBrokerProjectTag() { }

    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Slug { get; private set; }

    internal static ChainBrokerProjectTag Create(Guid projectId, string name, string? slug) => new()
    {
        Id = Guid.NewGuid(),
        ProjectId = projectId,
        Name = name,
        Slug = slug
    };
}
