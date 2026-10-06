namespace ThaiX.Domain.Aggregates.ChainBroker;

/// <summary>Reference to a VC fund that invested in a ChainBroker project.</summary>
public sealed class ChainBrokerProjectFundRef
{
    private ChainBrokerProjectFundRef() { }

    public Guid Id { get; private set; }
    public Guid ProjectId { get; private set; }
    public Guid FundId { get; private set; }

    public ChainBrokerProject Project { get; private set; } = null!;
    public ChainBrokerFund Fund { get; private set; } = null!;

    internal static ChainBrokerProjectFundRef Create(Guid projectId, Guid fundId) => new()
    {
        Id = Guid.NewGuid(),
        ProjectId = projectId,
        FundId = fundId
    };
}
