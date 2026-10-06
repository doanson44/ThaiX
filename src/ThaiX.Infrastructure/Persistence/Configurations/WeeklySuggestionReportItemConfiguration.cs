using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.TradingSuggestions;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class WeeklySuggestionReportItemConfiguration : IEntityTypeConfiguration<WeeklySuggestionReportItem>
{
    public void Configure(EntityTypeBuilder<WeeklySuggestionReportItem> builder)
    {
        builder.ToTable("WeeklySuggestionReportItems");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Timeframe).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Rank).IsRequired();
        builder.Property(x => x.Symbol).IsRequired().HasMaxLength(32);
        builder.Property(x => x.MarketType).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.CompositeScore).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.Signal).IsRequired().HasMaxLength(20);
        builder.Property(x => x.Confidence).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.EntryPrice).HasPrecision(18, 4).IsRequired();
        builder.Property(x => x.StopLoss).HasPrecision(18, 4);
        builder.Property(x => x.TakeProfit1).HasPrecision(18, 4);
        builder.Property(x => x.TakeProfit2).HasPrecision(18, 4);

        builder.HasIndex(x => new { x.ReportId, x.Timeframe, x.Rank }).IsUnique();
    }
}
