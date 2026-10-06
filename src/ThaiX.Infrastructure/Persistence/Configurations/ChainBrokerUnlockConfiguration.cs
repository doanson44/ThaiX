using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.ChainBroker;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class ChainBrokerUnlockConfiguration : IEntityTypeConfiguration<ChainBrokerUnlock>
{
    public void Configure(EntityTypeBuilder<ChainBrokerUnlock> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(200);

        builder.HasIndex(x => x.Slug);
        builder.HasIndex(x => new { x.Slug, x.NextUnlockDate }).IsUnique();

        builder.Property(x => x.Name).HasMaxLength(200);
        builder.Property(x => x.Logo).HasMaxLength(2048);
        builder.Property(x => x.Ticker).HasMaxLength(50);
        builder.Property(x => x.RoundName).HasMaxLength(500);
        builder.Property(x => x.UnlockAmount).HasMaxLength(100);

        builder.Property(x => x.UnlockValueUsd).HasPrecision(28, 6);
        builder.Property(x => x.CirculationPercent).HasPrecision(10, 4);
        builder.Property(x => x.PriceChange24h).HasPrecision(10, 4);
        builder.Property(x => x.PriceChange7d).HasPrecision(10, 4);
        builder.Property(x => x.PriceChange30d).HasPrecision(10, 4);
        builder.Property(x => x.PriceChange1y).HasPrecision(10, 4);
        builder.Property(x => x.Volume24hUsd).HasPrecision(28, 6);
        builder.Property(x => x.UnlockPercent).HasPrecision(10, 4);
    }
}
