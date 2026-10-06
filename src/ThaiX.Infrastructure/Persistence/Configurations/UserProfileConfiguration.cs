using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Application.Common.Models;

namespace ThaiX.Infrastructure.Persistence.Configurations;

internal sealed class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.HasKey(e => e.UserId);

        builder.Property(e => e.ContactId).IsRequired();
        builder.Property(e => e.FullName).IsRequired().HasMaxLength(512);
        builder.Property(e => e.AvatarUrl).HasMaxLength(2048);
        builder.Property(e => e.UpdatedAt).IsRequired();

        builder.HasIndex(e => e.ContactId)
            .HasDatabaseName("IX_UserProfiles_ContactId");
    }
}
