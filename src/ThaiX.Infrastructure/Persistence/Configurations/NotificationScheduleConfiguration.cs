using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using ThaiX.Domain.Aggregates.Notifications;
using ThaiX.Domain.Aggregates.Notifications.ValueObjects;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class NotificationScheduleConfiguration : IEntityTypeConfiguration<NotificationSchedule>
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public void Configure(EntityTypeBuilder<NotificationSchedule> builder)
    {
        builder.ToTable("NotificationSchedules");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Description).HasMaxLength(1024);
        builder.Property(x => x.TemplateKey).IsRequired().HasMaxLength(128);
        builder.Property(x => x.Subject).IsRequired().HasMaxLength(256);
        builder.Property(x => x.Body).IsRequired().HasMaxLength(20000);
        builder.Property(x => x.DataJson);

        builder.Property(x => x.Recurrence)
            .IsRequired()
            .HasConversion(
                v => JsonSerializer.Serialize(v, JsonOptions),
                v => JsonSerializer.Deserialize<ScheduleRecurrence>(v, JsonOptions)!,
                new ValueComparer<ScheduleRecurrence>(
                    (a, b) => JsonSerializer.Serialize(a, JsonOptions) == JsonSerializer.Serialize(b, JsonOptions),
                    v => JsonSerializer.Serialize(v, JsonOptions).GetHashCode(),
                    v => JsonSerializer.Deserialize<ScheduleRecurrence>(JsonSerializer.Serialize(v, JsonOptions), JsonOptions)!));

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.TimeZoneId).IsRequired().HasMaxLength(128);

        builder.Property(x => x.ExecuteTimeLocal).HasColumnType("time");
        builder.Property(x => x.OneTimeAtLocal);
        builder.Property(x => x.StartDateLocal);
        builder.Property(x => x.StartAtUtc);
        builder.Property(x => x.EndAtUtc);
        builder.Property(x => x.LastTriggeredAtUtc);
        builder.Property(x => x.LastSuccessfulAtUtc);
        builder.Property(x => x.NextExecuteAtUtc);

        // Index for due schedule polling
        builder.HasIndex(x => new { x.Status, x.NextExecuteAtUtc })
            .HasFilter("[Status] = 'Active' AND [NextExecuteAtUtc] IS NOT NULL");
    }
}
