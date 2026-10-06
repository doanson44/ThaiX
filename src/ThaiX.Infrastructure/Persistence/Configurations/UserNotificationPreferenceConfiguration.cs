using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class UserNotificationPreferenceConfiguration : IEntityTypeConfiguration<UserNotificationPreference>
{
    public void Configure(EntityTypeBuilder<UserNotificationPreference> builder)
    {
        builder.ToTable("UserNotificationPreferences");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Kind)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.Property(x => x.Channel)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.Property(x => x.Enabled)
            .IsRequired();

        builder.Property(x => x.Destination)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.MinimumSeverity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.TimeZoneId)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.BatchingMode)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.Kind, x.Channel })
            .IsUnique();
        builder.HasIndex(x => new { x.Kind, x.Channel, x.Enabled });
    }
}
