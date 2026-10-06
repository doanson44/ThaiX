namespace ThaiX.Domain.Aggregates.ChainBroker;

/// <summary>Blockchain network that a ChainBroker project runs on.</summary>
public sealed class ChainBrokerProjectBlockchain
{
    private ChainBrokerProjectBlockchain() { }

    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    internal static ChainBrokerProjectBlockchain Create(Guid projectId, string name) => new()
    {
        Id = Guid.NewGuid(),
        ProjectId = projectId,
        Name = name
    };
}
