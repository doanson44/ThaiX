using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.ChainBroker;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class ChainBrokerFundConfiguration : IEntityTypeConfiguration<ChainBrokerFund>
{
    public void Configure(EntityTypeBuilder<ChainBrokerFund> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug).IsRequired().HasMaxLength(200);
        builder.HasIndex(x => x.Slug).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.Logo).HasMaxLength(2048);
        builder.Property(x => x.FundTypeName).HasMaxLength(100);
        builder.Property(x => x.FundTypeSlug).HasMaxLength(100);
        builder.Property(x => x.Status).HasMaxLength(50);

        // ROI
        builder.Property(x => x.AverageCurrentRoi).HasPrecision(18, 4);

        // Market cap / fundraising averages
        builder.Property(x => x.AverageMarketCapUsd).HasPrecision(28, 2);
        builder.Property(x => x.AverageInitialMarketCapUsd).HasPrecision(28, 2);
        builder.Property(x => x.AverageFdmcUsd).HasPrecision(28, 2);
        builder.Property(x => x.AverageInitialFdmcUsd).HasPrecision(28, 2);
        builder.Property(x => x.AveragePublicRaiseUsd).HasPrecision(28, 2);
        builder.Property(x => x.AveragePrivateRaiseUsd).HasPrecision(28, 2);
        builder.Property(x => x.AverageTotalRaiseUsd).HasPrecision(28, 2);

        // Price change averages
        builder.Property(x => x.AveragePriceChange24h).HasPrecision(10, 4);
        builder.Property(x => x.AveragePriceChange7d).HasPrecision(10, 4);
        builder.Property(x => x.AveragePriceChange30d).HasPrecision(10, 4);
        builder.Property(x => x.AveragePriceChange1y).HasPrecision(10, 4);

        // Gainers/losers
        builder.Property(x => x.GainersPercent).HasPrecision(10, 4);
        builder.Property(x => x.LosersPercent).HasPrecision(10, 4);
    }
}
