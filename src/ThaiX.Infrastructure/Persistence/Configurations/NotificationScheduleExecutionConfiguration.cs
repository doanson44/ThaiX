using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.Notifications;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class NotificationScheduleExecutionConfiguration : IEntityTypeConfiguration<NotificationScheduleExecution>
{
    public void Configure(EntityTypeBuilder<NotificationScheduleExecution> builder)
    {
        builder.ToTable("NotificationScheduleExecutions");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(32);

        builder.Property(x => x.OccurrenceTimeUtc).IsRequired();
        builder.Property(x => x.TriggeredAtUtc).IsRequired();
        builder.Property(x => x.CompletedAtUtc);
        builder.Property(x => x.NotificationId);
        builder.Property(x => x.ErrorCode).HasMaxLength(128);
        builder.Property(x => x.ErrorMessage).HasMaxLength(2048);

        builder.HasIndex(x => x.ScheduleId);
        builder.HasIndex(x => x.TriggeredAtUtc);

        builder.HasOne(x => x.Schedule)
            .WithMany()
            .HasForeignKey(x => x.ScheduleId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
