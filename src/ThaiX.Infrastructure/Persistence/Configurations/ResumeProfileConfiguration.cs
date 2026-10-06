using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.Resumes;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class ResumeProfileConfiguration : IEntityTypeConfiguration<ResumeProfile>
{
    public void Configure(EntityTypeBuilder<ResumeProfile> builder)
    {
        builder.ToTable("ResumeProfiles");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.OwnerId)
            .IsRequired();

        builder.Property(x => x.Slug)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.FullName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Headline)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.MetaDescription)
            .HasMaxLength(300);

        builder.Property(x => x.IsPublished)
            .IsRequired();

        builder.Property(x => x.ContentJson)
            .IsRequired();

        builder.HasIndex(x => x.Slug).IsUnique();
        builder.HasIndex(x => x.OwnerId);
    }
}
