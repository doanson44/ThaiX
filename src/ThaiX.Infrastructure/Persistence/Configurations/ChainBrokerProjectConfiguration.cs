using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.ChainBroker;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class ChainBrokerProjectConfiguration : IEntityTypeConfiguration<ChainBrokerProject>
{
    public void Configure(EntityTypeBuilder<ChainBrokerProject> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => x.Slug).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.Logo).HasMaxLength(2048);
        builder.Property(x => x.Ticker).HasMaxLength(50);

        // Prices
        builder.Property(x => x.CurrentPriceUsd).HasPrecision(28, 10);
        builder.Property(x => x.PublicPriceUsd).HasPrecision(28, 10);
        builder.Property(x => x.PrivatePriceUsd).HasPrecision(28, 10);
        builder.Property(x => x.AthPriceUsd).HasPrecision(28, 10);
        builder.Property(x => x.AtlPriceUsd).HasPrecision(28, 10);

        // ROI
        builder.Property(x => x.PublicRoi).HasPrecision(18, 4);
        builder.Property(x => x.PrivateRoi).HasPrecision(18, 4);
        builder.Property(x => x.PublicAthRoi).HasPrecision(18, 4);
        builder.Property(x => x.PrivateAthRoi).HasPrecision(18, 4);

        // Scores
        builder.Property(x => x.BrokerScore).HasPrecision(8, 4);
        builder.Property(x => x.SecurityScore).HasPrecision(8, 4);

        // Fundraising
        builder.Property(x => x.PublicRaiseUsd).HasPrecision(28, 2);
        builder.Property(x => x.PrivateRaiseUsd).HasPrecision(28, 2);
        builder.Property(x => x.TotalRaiseUsd).HasPrecision(28, 2);

        // Market data
        builder.Property(x => x.MarketCapUsd).HasPrecision(28, 2);
        builder.Property(x => x.ReportedMarketCapUsd).HasPrecision(28, 2);
        builder.Property(x => x.InitialMarketCapUsd).HasPrecision(28, 2);
        builder.Property(x => x.FdmcUsd).HasPrecision(28, 2);
        builder.Property(x => x.InitialFdmcUsd).HasPrecision(28, 2);
        builder.Property(x => x.Volume24hUsd).HasPrecision(28, 2);

        // Circulation
        builder.Property(x => x.CurrentCirculation).HasPrecision(28, 2);
        builder.Property(x => x.InitialCirculation).HasPrecision(28, 2);
        builder.Property(x => x.TotalCirculation).HasPrecision(28, 2);
        builder.Property(x => x.PercentCirculating).HasPrecision(10, 4);

        // Price changes
        builder.Property(x => x.PriceChange24h).HasPrecision(10, 4);
        builder.Property(x => x.PriceChange7d).HasPrecision(10, 4);
        builder.Property(x => x.PriceChange30d).HasPrecision(10, 4);
        builder.Property(x => x.PriceChange1y).HasPrecision(10, 4);

        // Children
        builder.HasMany(x => x.Blockchains)
            .WithOne()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Tags)
            .WithOne()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Funds)
            .WithOne()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Launchpads)
            .WithOne()
            .HasForeignKey(e => e.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}

public sealed class ChainBrokerProjectBlockchainConfiguration
    : IEntityTypeConfiguration<ChainBrokerProjectBlockchain>
{
    public void Configure(EntityTypeBuilder<ChainBrokerProjectBlockchain> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProjectId).IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(100);
        builder.HasIndex(x => x.ProjectId);
    }
}

public sealed class ChainBrokerProjectTagConfiguration
    : IEntityTypeConfiguration<ChainBrokerProjectTag>
{
    public void Configure(EntityTypeBuilder<ChainBrokerProjectTag> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProjectId).IsRequired();
        builder.Property(x => x.Name).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Slug).HasMaxLength(200);
        builder.HasIndex(x => x.ProjectId);
    }
}

public sealed class ChainBrokerProjectFundRefConfiguration
    : IEntityTypeConfiguration<ChainBrokerProjectFundRef>
{
    public void Configure(EntityTypeBuilder<ChainBrokerProjectFundRef> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProjectId).IsRequired();
        builder.Property(x => x.FundId).IsRequired();

        builder.HasIndex(x => new { x.ProjectId, x.FundId })
            .IsUnique();

        builder.HasIndex(x => x.ProjectId);
        builder.HasIndex(x => x.FundId);
        builder.HasOne(x => x.Project)
            .WithMany(p => p.Funds)
            .HasForeignKey(x => x.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(x => x.Fund)
            .WithMany()
            .HasForeignKey(x => x.FundId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired();
    }
}

public sealed class ChainBrokerProjectLaunchpadRefConfiguration
    : IEntityTypeConfiguration<ChainBrokerProjectLaunchpadRef>
{
    public void Configure(EntityTypeBuilder<ChainBrokerProjectLaunchpadRef> builder)
    {
        builder.HasKey(x => x.Id);
        builder.Property(x => x.ProjectId).IsRequired();
        builder.Property(x => x.Slug).IsRequired().HasMaxLength(200);
        builder.Property(x => x.Name).HasMaxLength(200);
        builder.HasIndex(x => x.ProjectId);
    }
}
