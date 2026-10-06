using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.CredentialAccounts;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class CredentialAccountConfiguration : IEntityTypeConfiguration<CredentialAccount>
{
    public void Configure(EntityTypeBuilder<CredentialAccount> builder)
    {
        builder.ToTable("CredentialAccounts");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.Username)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.PasswordEncrypted)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.IsUsed)
            .IsRequired();

        builder.Property(x => x.UsedBy)
            .HasMaxLength(256);

        builder.Property(x => x.UsageCount)
            .IsRequired();

        builder.HasIndex(x => x.Username)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0")
            .HasDatabaseName("IX_CredentialAccount_Username");

        builder.HasIndex(x => x.IsUsed)
            .HasDatabaseName("IX_CredentialAccount_IsUsed");

        builder.HasIndex(x => x.UsedAt)
            .HasDatabaseName("IX_CredentialAccount_UsedAt");
    }
}
