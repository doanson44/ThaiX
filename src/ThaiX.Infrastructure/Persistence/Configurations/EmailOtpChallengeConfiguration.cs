using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Infrastructure.Identity;
using ThaiX.Infrastructure.Persistence.Entities;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class EmailOtpChallengeConfiguration : IEntityTypeConfiguration<EmailOtpChallenge>
{
    public void Configure(EntityTypeBuilder<EmailOtpChallenge> builder)
    {
        builder.ToTable("EmailOtpChallenges");
        builder.HasKey(x => x.Id);
        builder.Property(x => x.CodeHash).HasMaxLength(512).IsRequired();
        builder.Property(x => x.CreatedAtUtc).IsRequired();
        builder.Property(x => x.ExpiresAtUtc).IsRequired();
        builder.Property(x => x.FailedAttempts).IsRequired();
        builder.HasOne<ApplicationUser>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasIndex(x => x.UserId);
        builder.HasIndex(x => new { x.UserId, x.CreatedAtUtc });
        builder.HasIndex(x => x.ExpiresAtUtc);
        builder.HasIndex(x => new { x.UserId, x.ConsumedAtUtc, x.InvalidatedAtUtc });
    }
}
