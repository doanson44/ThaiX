using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class ContactEmailConfiguration : IEntityTypeConfiguration<ContactEmail>
{
    public void Configure(EntityTypeBuilder<ContactEmail> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContactId).IsRequired();

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(320);

        builder.Property(x => x.IsPrimary).IsRequired();

        builder.HasIndex(x => x.ContactId);
        builder.HasIndex(x => x.Value);
    }
}
