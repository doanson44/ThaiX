using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.AssetPositions;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class CryptoPositionConfiguration : IEntityTypeConfiguration<CryptoPosition>
{
    public void Configure(EntityTypeBuilder<CryptoPosition> builder)
    {
        builder.ToTable("CryptoPositions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PortfolioId)
            .IsRequired();

        builder.Property(x => x.Symbol)
            .IsRequired()
            .HasMaxLength(32);

        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasPrecision(28, 10);

        builder.Property(x => x.AverageEntryPrice)
            .IsRequired()
            .HasPrecision(28, 10);

        builder.Property(x => x.TotalInvested)
            .IsRequired()
            .HasPrecision(28, 10);

        builder.Property(x => x.RealizedPnl)
            .IsRequired()
            .HasPrecision(28, 10);

        builder.Property(x => x.TargetPrice)
            .HasPrecision(28, 10);

        builder.Property(x => x.StopLoss)
            .HasPrecision(28, 10);

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.HasOne(x => x.Portfolio)
            .WithMany()
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(x => x.Transactions)
            .WithOne()
            .HasForeignKey("CryptoPositionId")
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Transactions).HasField("_transactions");

        builder.HasIndex(x => x.PortfolioId);
        builder.HasIndex(x => new { x.PortfolioId, x.Symbol });
    }
}
