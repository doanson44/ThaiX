using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class NotificationDeliveryConfiguration : IEntityTypeConfiguration<NotificationDelivery>
{
    public void Configure(EntityTypeBuilder<NotificationDelivery> builder)
    {
        builder.ToTable("NotificationDeliveries");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Channel)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(64);

        builder.Property(x => x.Destination)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.Provider)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.AttemptCount)
            .IsRequired();

        builder.Property(x => x.MaxAttempts)
            .IsRequired();

        builder.Property(x => x.ProviderMessageId)
            .HasMaxLength(256);

        builder.Property(x => x.ErrorCode)
            .HasMaxLength(128);

        builder.Property(x => x.ErrorMessage)
            .HasMaxLength(2000);

        builder.Property(x => x.IdempotencyKey)
            .IsRequired()
            .HasMaxLength(128);

        builder.Property(x => x.RenderedPayloadJson);

        builder.HasIndex(x => x.NotificationId);
        builder.HasIndex(x => x.Channel);
        builder.HasIndex(x => new { x.Status, x.NextAttemptAtUtc });
        builder.HasIndex(x => x.IdempotencyKey)
            .IsUnique();
    }
}
