using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class ContactPhoneConfiguration : IEntityTypeConfiguration<ContactPhone>
{
    public const int ValueMaxLength = 20;
    public const int NormalizedValueMaxLength = 20;

    public void Configure(EntityTypeBuilder<ContactPhone> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContactId).IsRequired();

        builder.Property(x => x.Value)
            .IsRequired()
            .HasMaxLength(ValueMaxLength);

        builder.Property(x => x.NormalizedValue)
            .IsRequired()
            .HasMaxLength(NormalizedValueMaxLength);

        builder.Property(x => x.IsPrimary).IsRequired();

        builder.HasIndex(x => x.ContactId);
        builder.HasIndex(x => x.NormalizedValue)
            .IsUnique()
            .HasFilter("[NormalizedValue] <> ''");
    }
}
