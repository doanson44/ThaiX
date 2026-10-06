using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.AssetPositions;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class StockPositionConfiguration : IEntityTypeConfiguration<StockPosition>
{
    public void Configure(EntityTypeBuilder<StockPosition> builder)
    {
        builder.ToTable("StockPositions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PortfolioId)
            .IsRequired();

        builder.Property(x => x.Symbol)
            .IsRequired()
            .HasMaxLength(16);

        builder.Property(x => x.Exchange)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasPrecision(18, 6);

        builder.Property(x => x.AverageEntryPrice)
            .IsRequired()
            .HasPrecision(18, 6);

        builder.Property(x => x.TotalInvested)
            .IsRequired()
            .HasPrecision(18, 6);

        builder.Property(x => x.RealizedPnl)
            .IsRequired()
            .HasPrecision(18, 6);

        builder.Property(x => x.TargetPrice)
            .HasPrecision(18, 6);

        builder.Property(x => x.StopLoss)
            .HasPrecision(18, 6);

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.HasOne(x => x.Portfolio)
            .WithMany()
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Transactions)
            .WithOne()
            .HasForeignKey("StockPositionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Transactions).HasField("_transactions");

        builder.HasIndex(x => x.PortfolioId);
        builder.HasIndex(x => new { x.PortfolioId, x.Symbol });
    }
}
