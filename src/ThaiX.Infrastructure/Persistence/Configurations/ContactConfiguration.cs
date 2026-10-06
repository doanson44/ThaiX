using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Text.Json;
using ThaiX.Domain.Aggregates.Contacts;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.HasKey(x => x.Id);

        // FullName as owned value object (columns on Contact table)
        builder.OwnsOne(x => x.FullName, fn =>
        {
            fn.Property(f => f.FirstName)
                .HasColumnName("FirstName")
                .IsRequired()
                .HasMaxLength(100);

            fn.Property(f => f.LastName)
                .HasColumnName("LastName")
                .IsRequired()
                .HasMaxLength(100);
        });

        builder.Navigation(x => x.FullName).IsRequired();

        builder.Property(x => x.Company).HasMaxLength(256);
        builder.Property(x => x.JobTitle).HasMaxLength(256);
        builder.Property(x => x.AvatarUrl).HasMaxLength(2048);
        builder.Property(x => x.Birthday);
        builder.Property(x => x.Notes).HasMaxLength(4000);
        builder.Property(x => x.IsArchived).IsRequired().HasDefaultValue(false);

        // CustomFields stored as JSON
        builder.Property(x => x.CustomFields)
            .HasConversion(
                v => JsonSerializer.Serialize(v, (JsonSerializerOptions?)null),
                v => JsonSerializer.Deserialize<Dictionary<string, string>>(v, (JsonSerializerOptions?)null)
                     ?? new Dictionary<string, string>());

        // Collection relationships
        builder.HasMany(x => x.Emails)
            .WithOne()
            .HasForeignKey(e => e.ContactId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Phones)
            .WithOne()
            .HasForeignKey(p => p.ContactId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Addresses)
            .WithOne()
            .HasForeignKey(a => a.ContactId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.SocialLinks)
            .WithOne()
            .HasForeignKey(s => s.ContactId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.Tags)
            .WithOne()
            .HasForeignKey(t => t.ContactId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(x => x.BankAccounts)
            .WithOne()
            .HasForeignKey(ba => ba.ContactId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.IdentityDocument)
            .WithOne()
            .HasForeignKey<ContactIdentityDocument>(d => d.ContactId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(x => x.IsArchived);
        builder.HasIndex(x => x.Company);
    }
}
