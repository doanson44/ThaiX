using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class ContactAddressConfiguration : IEntityTypeConfiguration<ContactAddress>
{
    public void Configure(EntityTypeBuilder<ContactAddress> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContactId).IsRequired();

        builder.Property(x => x.Street)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.CountryCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(x => x.CityCode)
            .HasMaxLength(20);

        builder.Property(x => x.DistrictCode)
            .HasMaxLength(20);

        builder.Property(x => x.PostalCode)
            .HasMaxLength(20);

        builder.Property(x => x.IsPrimary).IsRequired();

        builder.HasIndex(x => x.ContactId);
    }
}
