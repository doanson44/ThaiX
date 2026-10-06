using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class TransferConfiguration : IEntityTypeConfiguration<Transfer>
{
    public void Configure(EntityTypeBuilder<Transfer> builder)
    {
        builder.ToTable("Transfers");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.HasOne(x => x.SourceWallet)
            .WithMany()
            .HasForeignKey(x => x.SourceWalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.TargetWallet)
            .WithMany()
            .HasForeignKey(x => x.TargetWalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.SourceWalletId, x.TransferredOn });
        builder.HasIndex(x => new { x.TargetWalletId, x.TransferredOn });
    }
}
