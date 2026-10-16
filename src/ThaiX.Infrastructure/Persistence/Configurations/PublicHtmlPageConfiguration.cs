using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.PublicHtml;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class PublicHtmlPageConfiguration : IEntityTypeConfiguration<PublicHtmlPage>
{
    public void Configure(EntityTypeBuilder<PublicHtmlPage> builder)
    {
        builder.ToTable("PublicHtmlPages");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Title)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.StorageKey)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.SizeBytes)
            .IsRequired();

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.Property(x => x.IsPublished)
            .IsRequired()
            .HasDefaultValue(false);

        builder.HasIndex(x => x.IsPublished);
        builder.HasIndex(x => x.LastViewedAtUtc);

    }
}
