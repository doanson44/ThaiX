using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.TcbsTop10;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class TcbsTop10ImageConfiguration : IEntityTypeConfiguration<TcbsTop10Image>
{
    public void Configure(EntityTypeBuilder<TcbsTop10Image> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PortfolioId).IsRequired();
        builder.Property(x => x.ImageType).IsRequired();
        builder.Property(x => x.FileGuid).IsRequired();

        builder.HasIndex(x => new { x.PortfolioId, x.ImageType }).IsUnique();
    }
}
