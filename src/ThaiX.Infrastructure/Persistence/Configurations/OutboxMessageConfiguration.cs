using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.Outbox;

namespace ThaiX.Infrastructure.Persistence.Configurations;

internal sealed class OutboxMessageConfiguration : IEntityTypeConfiguration<OutboxMessage>
{
    public void Configure(EntityTypeBuilder<OutboxMessage> builder)
    {
        builder.ToTable("OutboxMessages");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Type)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(e => e.Content)
            .IsRequired();

        builder.Property(e => e.OccurredOnUtc)
            .IsRequired();

        builder.Property(e => e.ProcessedOnUtc);

        builder.Property(e => e.Error);

        builder.HasIndex(e => e.ProcessedOnUtc)
            .HasDatabaseName("IX_OutboxMessages_ProcessedOnUtc");

        builder.HasIndex(e => e.OccurredOnUtc)
            .HasDatabaseName("IX_OutboxMessages_OccurredOnUtc");
    }
}
