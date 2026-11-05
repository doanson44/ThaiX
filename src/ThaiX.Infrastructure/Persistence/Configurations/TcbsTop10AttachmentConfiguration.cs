using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.TcbsTop10;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class TcbsTop10AttachmentConfiguration : IEntityTypeConfiguration<TcbsTop10Attachment>
{
    public void Configure(EntityTypeBuilder<TcbsTop10Attachment> builder)
    {
        // Legacy table name kept so the rename from TcbsTop10Image does not drop existing image rows.
        builder.ToTable("TcbsTop10Images");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.PortfolioId).IsRequired();
        builder.Property(x => x.Kind).IsRequired()
            .HasConversion<string>().HasMaxLength(32)
            .HasDefaultValue(TcbsAttachmentKind.Image);
        builder.Property(x => x.ImageType).HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Url).HasMaxLength(512);

        // Kept from the original schema: also backs the Portfolio FK index.
        builder.HasIndex(x => new { x.PortfolioId, x.ImageType }).IsUnique();
    }
}
