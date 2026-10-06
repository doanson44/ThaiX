using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class ContactBankAccountConfiguration : IEntityTypeConfiguration<ContactBankAccount>
{
    public void Configure(EntityTypeBuilder<ContactBankAccount> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ContactId).IsRequired();

        builder.Property(x => x.BankCode)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(x => x.BranchName)
            .HasMaxLength(20);

        builder.Property(x => x.EncryptedAccountNumber)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.AccountNumberLast4)
            .IsRequired()
            .HasMaxLength(4);

        builder.Property(x => x.AccountName)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.CurrencyCode)
            .IsRequired()
            .HasMaxLength(3);

        builder.Property(x => x.IsPrimary).IsRequired();
        builder.Property(x => x.IsVerified).IsRequired().HasDefaultValue(false);

        builder.HasIndex(x => x.ContactId);
    }
}
