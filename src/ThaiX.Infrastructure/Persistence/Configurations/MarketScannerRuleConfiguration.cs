using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.MarketScanner;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class MarketScannerRuleConfiguration : IEntityTypeConfiguration<MarketScannerRule>
{
    public void Configure(EntityTypeBuilder<MarketScannerRule> builder)
    {
        builder.ToTable("MarketScannerRules");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SignalType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.Window)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(x => x.Threshold)
            .HasPrecision(18, 6);

        builder.Property(x => x.IsEnabled)
            .IsRequired();

        builder.HasIndex(x => new { x.SignalType, x.Window });
        builder.HasIndex(x => x.IsEnabled);
    }
}
