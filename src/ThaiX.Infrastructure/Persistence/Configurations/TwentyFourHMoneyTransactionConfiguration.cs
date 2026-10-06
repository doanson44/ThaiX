using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.MarketData;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class TwentyFourHMoneyTransactionConfiguration : IEntityTypeConfiguration<TwentyFourHMoneyTransaction>
{
    public void Configure(EntityTypeBuilder<TwentyFourHMoneyTransaction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Symbol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.TradeTime)
            .IsRequired()
            .HasMaxLength(8);

        builder.Property(x => x.Side)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.Price).HasPrecision(18, 4);
        builder.Property(x => x.Change).HasPrecision(18, 4);

        // TotalVolume is a cumulative running total within a trading day, so it is
        // effectively unique per (Symbol, TradeDate) — used to dedupe/re-run the sync job safely.
        builder.HasIndex(x => new { x.Symbol, x.TradeDate, x.TotalVolume }).IsUnique();

        // Supports the daily retention purge (delete rows older than 1 year).
        builder.HasIndex(x => x.TradeDate);
    }
}
