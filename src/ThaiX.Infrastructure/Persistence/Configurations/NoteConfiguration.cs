using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.Notes;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class NoteConfiguration : IEntityTypeConfiguration<Note>
{
    public void Configure(EntityTypeBuilder<Note> builder)
    {
        builder.ToTable("Notes");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OwnerId)
            .IsRequired();

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Content)
            .IsRequired()
            .HasMaxLength(10000);

        builder.Property(x => x.Color)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(16);

        builder.Property(x => x.IsPinned)
            .IsRequired();

        builder.Property(x => x.IsArchived)
            .IsRequired();

        builder.HasIndex(x => x.OwnerId);
        builder.HasIndex(x => new { x.OwnerId, x.IsPinned });
        builder.HasIndex(x => new { x.OwnerId, x.IsArchived });
    }
}
