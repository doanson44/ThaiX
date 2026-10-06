using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.TcbsTop10;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class TcbsTop10TickerConfiguration : IEntityTypeConfiguration<TcbsTop10Ticker>
{
    public void Configure(EntityTypeBuilder<TcbsTop10Ticker> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PortfolioId).IsRequired();
        builder.Property(x => x.Ticker).IsRequired().HasMaxLength(20);
        builder.Property(x => x.ChangeType).IsRequired();

        builder.HasIndex(x => new { x.PortfolioId, x.Ticker, x.ChangeType }).IsUnique();
    }
}
