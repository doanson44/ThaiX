using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.CredentialAccounts;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class CredentialAccountAuditConfiguration : IEntityTypeConfiguration<CredentialAccountAudit>
{
    public void Configure(EntityTypeBuilder<CredentialAccountAudit> builder)
    {
        builder.ToTable("CredentialAccountAudits");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Action)
            .HasConversion<string>()
            .HasMaxLength(64)
            .IsRequired();

        builder.Property(x => x.UserName)
            .HasMaxLength(256)
            .IsRequired();

        builder.HasIndex(x => new { x.CredentialAccountId, x.CreatedAt });

        builder.HasOne<CredentialAccount>()
            .WithMany()
            .HasForeignKey(x => x.CredentialAccountId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
