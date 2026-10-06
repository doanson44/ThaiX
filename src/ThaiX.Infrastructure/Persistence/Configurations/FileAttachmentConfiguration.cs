using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Common.Entities;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class FileAttachmentConfiguration : IEntityTypeConfiguration<FileAttachment>
{
    public void Configure(EntityTypeBuilder<FileAttachment> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.StorageKey)
            .IsRequired()
            .HasMaxLength(1024);

        builder.Property(x => x.FileName)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Size)
            .IsRequired();

        builder.HasIndex(x => x.StorageKey).IsUnique();
    }
}
