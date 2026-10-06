using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.AssetPositions;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class SavingPositionConfiguration : IEntityTypeConfiguration<SavingPosition>
{
    public void Configure(EntityTypeBuilder<SavingPosition> builder)
    {
        builder.ToTable("SavingPositions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PortfolioId)
            .IsRequired();

        builder.Property(x => x.BankName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.AccountNumber)
            .HasMaxLength(50);

        builder.Property(x => x.PrincipalAmount)
            .IsRequired()
            .HasPrecision(18, 6);

        builder.Property(x => x.InterestRate)
            .IsRequired()
            .HasPrecision(8, 4);

        builder.Property(x => x.InterestType)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(x => x.DepositDate)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(24);

        builder.Property(x => x.Note)
            .HasMaxLength(500);

        builder.HasOne(x => x.Portfolio)
            .WithMany()
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.PortfolioId);
        builder.HasIndex(x => new { x.PortfolioId, x.Status });
    }
}
