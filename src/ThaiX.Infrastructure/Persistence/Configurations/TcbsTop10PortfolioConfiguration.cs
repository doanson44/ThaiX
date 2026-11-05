using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.TcbsTop10;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class TcbsTop10PortfolioConfiguration : IEntityTypeConfiguration<TcbsTop10Portfolio>
{
    public void Configure(EntityTypeBuilder<TcbsTop10Portfolio> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Source).IsRequired()
            .HasConversion<string>().HasMaxLength(32)
            .HasDefaultValue(TcbsPortfolioSource.IWealthClub);
        builder.Property(x => x.SourceContentId);
        builder.HasIndex(x => x.SourceContentId).IsUnique();
        builder.HasIndex(x => new { x.Source, x.PostedAt });

        builder.Property(x => x.PostedAt).IsRequired();
        builder.Property(x => x.EffectiveDate).IsRequired();

        builder.HasMany(x => x.Tickers)
            .WithOne(x => x.Portfolio)
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Attachments)
            .WithOne(x => x.Portfolio)
            .HasForeignKey(x => x.PortfolioId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
