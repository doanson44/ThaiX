using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class ContactTagConfiguration : IEntityTypeConfiguration<ContactTag>
{
    public void Configure(EntityTypeBuilder<ContactTag> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContactId).IsRequired();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(x => x.ContactId);

        // Unique tag name per contact
        builder.HasIndex(x => new { x.ContactId, x.Name }).IsUnique();
    }
}
