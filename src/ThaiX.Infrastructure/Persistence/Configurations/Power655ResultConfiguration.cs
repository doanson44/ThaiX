using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.Lottery;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class Power655ResultConfiguration : IEntityTypeConfiguration<Power655Result>
{
    public void Configure(EntityTypeBuilder<Power655Result> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.DayOfWeek)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(x => x.Num1).IsRequired();
        builder.Property(x => x.Num2).IsRequired();
        builder.Property(x => x.Num3).IsRequired();
        builder.Property(x => x.Num4).IsRequired();
        builder.Property(x => x.Num5).IsRequired();
        builder.Property(x => x.Num6).IsRequired();
        builder.Property(x => x.BonusNum).IsRequired();

        builder.HasIndex(x => x.DrawDate).IsUnique();
    }
}
