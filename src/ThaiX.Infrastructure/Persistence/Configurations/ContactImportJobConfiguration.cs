using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.ContactImport;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class ContactImportJobConfiguration : IEntityTypeConfiguration<ContactImportJob>
{
    public void Configure(EntityTypeBuilder<ContactImportJob> builder)
    {
        builder.ToTable("ContactImportJobs");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        builder.Property(x => x.FilePath).IsRequired().HasMaxLength(2048);
        builder.Property(x => x.BatchSize).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.CompletedAtUtc);
        builder.Property(x => x.TotalRows).IsRequired();
        builder.Property(x => x.InsertedCount).IsRequired();
        builder.Property(x => x.UpdatedCount).IsRequired();
        builder.Property(x => x.SkippedCount).IsRequired();
        builder.Property(x => x.ErrorMessage).HasMaxLength(4000);
    }
}
