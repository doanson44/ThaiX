using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.PriceAlerts;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class PriceAlertConfiguration : IEntityTypeConfiguration<PriceAlert>
{
    public void Configure(EntityTypeBuilder<PriceAlert> builder)
    {
        builder.ToTable("PriceAlerts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Symbol)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.AssetType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(x => x.Condition)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(x => x.TargetPrice)
            .IsRequired()
            .HasPrecision(18, 6);

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.Property(x => x.IsEnabled)
            .IsRequired();

        builder.Property(x => x.IsOneTime)
            .IsRequired();

        builder.Property(x => x.TriggerCount)
            .IsRequired();

        builder.HasIndex(x => new { x.AssetType, x.Symbol });
        builder.HasIndex(x => x.IsEnabled);
    }
}
