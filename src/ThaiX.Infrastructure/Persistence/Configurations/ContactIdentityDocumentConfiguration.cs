using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class ContactIdentityDocumentConfiguration : IEntityTypeConfiguration<ContactIdentityDocument>
{
    public void Configure(EntityTypeBuilder<ContactIdentityDocument> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContactId).IsRequired();

        builder.Property(x => x.DocumentType)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.EncryptedDocumentNumber)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.DocumentNumberLast4)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(x => x.IssuedBy)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.IssuedPlace)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.IssuedDate).IsRequired();
        builder.Property(x => x.ExpiryDate);

        builder.HasIndex(x => x.ContactId).IsUnique();
    }
}
