using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.ExpenseTracker;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class ExpenseTagConfiguration : IEntityTypeConfiguration<Tag>
{
    public void Configure(EntityTypeBuilder<Tag> builder)
    {
        builder.ToTable("ExpenseTags");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.ColorHex)
            .HasMaxLength(16);

        builder.HasIndex(x => x.Name).IsUnique();
    }
}
