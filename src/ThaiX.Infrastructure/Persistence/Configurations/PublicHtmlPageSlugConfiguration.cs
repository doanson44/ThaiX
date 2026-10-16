using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.PublicHtml;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class PublicHtmlPageSlugConfiguration : IEntityTypeConfiguration<PublicHtmlPageSlug>
{
    public void Configure(EntityTypeBuilder<PublicHtmlPageSlug> builder)
    {
        builder.ToTable("PublicHtmlPageSlugs");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(80);

        builder.Property(x => x.IsCanonical)
            .IsRequired();

        builder.HasIndex(x => x.Slug)
            .IsUnique();

        builder.HasIndex(x => new { x.PublicHtmlPageId, x.IsCanonical });
    }
}
