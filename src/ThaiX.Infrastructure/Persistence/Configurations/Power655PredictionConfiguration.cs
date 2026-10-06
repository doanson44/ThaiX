using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.Lottery;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class Power655PredictionConfiguration : IEntityTypeConfiguration<Power655Prediction>
{
    public void Configure(EntityTypeBuilder<Power655Prediction> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TargetDrawDate).IsRequired();

        builder.Property(x => x.PredictionType)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(x => x.Num1).IsRequired();
        builder.Property(x => x.Num2).IsRequired();
        builder.Property(x => x.Num3).IsRequired();
        builder.Property(x => x.Num4).IsRequired();
        builder.Property(x => x.Num5).IsRequired();
        builder.Property(x => x.Num6).IsRequired();

        builder.Property(x => x.Reasoning)
            .HasMaxLength(2000);

        builder.Property(x => x.TupleFrequency)
            .IsRequired()
            .HasDefaultValue(0);

        builder.Property(x => x.MatchedNumbers)
            .IsRequired()
            .HasDefaultValue(0);

        // One prediction per type per target draw date
        builder.HasIndex(x => new { x.TargetDrawDate, x.PredictionType }).IsUnique();

        // Optional FK to Power655Result — set after draw occurs
        builder.HasOne(x => x.Power655Result)
            .WithMany()
            .HasForeignKey(x => x.Power655ResultId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
