using ThaiX.Domain.Common.Entities;

namespace ThaiX.Domain.Aggregates.Portfolios;

/// <summary>
/// A logical grouping of asset positions belonging to a user.
/// A user may own multiple portfolios with different investment strategies.
/// </summary>
public sealed class Portfolio : BaseAuditableEntity
{
    private Portfolio()
    {
    }

    /// <summary>The user who owns this portfolio.</summary>
    public Guid OwnerId { get; private set; }

    /// <summary>Display name of the portfolio.</summary>
    public string Name { get; private set; } = null!;

    /// <summary>Optional description.</summary>
    public string? Description { get; private set; }

    /// <summary>Investment strategy / category of this portfolio.</summary>
    public PortfolioType PortfolioType { get; private set; }

    public static Portfolio Create(
        Guid ownerId,
        string name,
        PortfolioType portfolioType,
        string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Portfolio
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Name = name.Trim(),
            PortfolioType = portfolioType,
            Description = description?.Trim()
        };
    }

    public void Update(string name, PortfolioType portfolioType, string? description)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        Name = name.Trim();
        PortfolioType = portfolioType;
        Description = description?.Trim();
    }

    public void SoftDelete()
    {
        Delete();
    }
}
