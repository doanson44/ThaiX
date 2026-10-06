using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Identity;

namespace ThaiX.Infrastructure.Persistence.Configurations;

internal sealed class UserContactLinkConfiguration : IEntityTypeConfiguration<UserContactLink>
{
    public void Configure(EntityTypeBuilder<UserContactLink> builder)
    {
        builder.ToTable("UserContactLinks");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.UserId).IsRequired();
        builder.Property(e => e.ContactId).IsRequired();

        builder.HasIndex(e => e.UserId)
            .IsUnique()
            .HasDatabaseName("IX_UserContactLinks_UserId");

        builder.HasIndex(e => e.ContactId)
            .IsUnique()
            .HasDatabaseName("IX_UserContactLinks_ContactId");
    }
}
