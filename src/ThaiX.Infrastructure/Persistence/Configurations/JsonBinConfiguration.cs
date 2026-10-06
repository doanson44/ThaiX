using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.JsonBin;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class JsonBinConfiguration : IEntityTypeConfiguration<JsonBin>
{
    public void Configure(EntityTypeBuilder<JsonBin> builder)
    {
        builder.ToTable("JsonBins");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(JsonBin.CodeMaxLength);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.Category)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(x => x.Content)
            .IsRequired();

        builder.Property(x => x.ContentType)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.SizeBytes)
            .IsRequired();

        builder.Property(x => x.IsCompressed)
            .IsRequired();

        builder.Property(x => x.Tags)
            .HasMaxLength(500);

        builder.Property(x => x.ReferenceId);
        builder.Property(x => x.ExpiredAtUtc);
        builder.Property(x => x.CreatedAtUtc)
            .IsRequired();

        builder.Property(x => x.ShareToken)
            .HasMaxLength(JsonBin.ShareTokenMaxLength);

        builder.Property(x => x.ShareExpiresAtUtc);

        // Unique among active rows so soft-deleted codes can be reused.
        builder.HasIndex(x => x.Code)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");

        builder.HasIndex(x => x.ShareToken)
            .IsUnique()
            .HasFilter("[ShareToken] IS NOT NULL");

        builder.HasIndex(x => x.Name);
        builder.HasIndex(x => x.Category);
        builder.HasIndex(x => x.ReferenceId);
        builder.HasIndex(x => x.ExpiredAtUtc);
        builder.HasIndex(x => new { x.IsDeleted, x.Category, x.CreatedAtUtc });
    }
}
