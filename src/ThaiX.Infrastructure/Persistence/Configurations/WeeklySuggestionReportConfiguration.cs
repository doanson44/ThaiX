using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.TradingSuggestions;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class WeeklySuggestionReportConfiguration : IEntityTypeConfiguration<WeeklySuggestionReport>
{
    public void Configure(EntityTypeBuilder<WeeklySuggestionReport> builder)
    {
        builder.ToTable("WeeklySuggestionReports");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.RunAtUtc).IsRequired();
        builder.Property(x => x.ElapsedSeconds).IsRequired();
        builder.Property(x => x.CandidateScanLimit).IsRequired();
        builder.Property(x => x.TopCount).IsRequired();
        builder.Property(x => x.AssetClass).IsRequired().HasConversion<string>().HasMaxLength(16);
        builder.Property(x => x.ReportType).IsRequired().HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.Status).IsRequired().HasConversion<string>().HasMaxLength(32);
        builder.Property(x => x.ReportKey).IsRequired().HasMaxLength(120);

        builder.HasIndex(x => x.ReportKey).IsUnique();
        builder.HasMany(x => x.Items)
            .WithOne(x => x.Report)
            .HasForeignKey(x => x.ReportId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
