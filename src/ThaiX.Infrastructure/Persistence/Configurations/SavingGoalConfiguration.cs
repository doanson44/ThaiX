using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class SavingGoalConfiguration : IEntityTypeConfiguration<SavingGoal>
{
    public void Configure(EntityTypeBuilder<SavingGoal> builder)
    {
        builder.ToTable("SavingGoals");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.TargetAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(x => x.CurrentAmount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.HasOne(x => x.Wallet)
            .WithMany()
            .HasForeignKey(x => x.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => x.WalletId);
    }
}
