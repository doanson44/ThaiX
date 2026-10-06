using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Kind)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.Property(x => x.Severity)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.TemplateKey)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Subject)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Body)
            .IsRequired()
            .HasMaxLength(20000);

        builder.Property(x => x.DataJson);

        builder.Property(x => x.SourceEventId)
            .HasMaxLength(128);

        builder.Property(x => x.DeduplicationKey)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.HasMany(x => x.Deliveries)
            .WithOne(x => x.Notification)
            .HasForeignKey(x => x.NotificationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Navigation(x => x.Deliveries).HasField("_deliveries");

        builder.HasIndex(x => x.DeduplicationKey)
            .IsUnique();
        builder.HasIndex(x => x.Status);
        builder.HasIndex(x => x.ScheduledAtUtc);
        builder.HasIndex(x => x.Kind);
        builder.HasIndex(x => x.RecipientUserId);
    }
}
