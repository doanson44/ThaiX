using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class ContactSocialLinkConfiguration : IEntityTypeConfiguration<ContactSocialLink>
{
    public void Configure(EntityTypeBuilder<ContactSocialLink> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContactId).IsRequired();

        builder.Property(x => x.Platform)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.Url)
            .IsRequired()
            .HasMaxLength(2048);

        builder.HasIndex(x => x.ContactId);
    }
}
