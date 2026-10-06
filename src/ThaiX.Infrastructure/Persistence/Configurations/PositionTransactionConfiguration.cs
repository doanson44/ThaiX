using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.AssetPositions;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class PositionTransactionConfiguration : IEntityTypeConfiguration<PositionTransaction>
{
    public void Configure(EntityTypeBuilder<PositionTransaction> builder)
    {
        builder.ToTable("PositionTransactions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PositionId)
            .IsRequired();

        builder.Property(x => x.AssetType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(x => x.TransactionType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(24);

        builder.Property(x => x.Quantity)
            .IsRequired()
            .HasPrecision(28, 10);

        builder.Property(x => x.Price)
            .IsRequired()
            .HasPrecision(28, 10);

        builder.Property(x => x.Fee)
            .IsRequired()
            .HasPrecision(28, 10);

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.Property(x => x.ExternalRef)
            .HasMaxLength(128);

        builder.HasIndex(x => x.PositionId);
        builder.HasIndex(x => new { x.PositionId, x.AssetType });
        builder.HasIndex(x => x.TransactedAt);
    }
}
