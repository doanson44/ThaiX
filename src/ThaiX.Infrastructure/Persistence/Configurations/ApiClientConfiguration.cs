using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ThaiX.Domain.Aggregates.ApiClient;

namespace ThaiX.Infrastructure.Persistence.Configurations;

public sealed class ApiClientConfiguration : IEntityTypeConfiguration<ApiClient>
{
    public void Configure(EntityTypeBuilder<ApiClient> builder)
    {
        builder.HasKey(x => x.Id);

        builder.Property(x => x.ClientId)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(x => x.ClientId)
            .IsUnique();

        builder.Property(x => x.ClientSecretHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(x => x.Description)
            .HasMaxLength(1000);

        builder.Property(x => x.IsActive)
            .IsRequired();

        // BaseEntity and BaseAuditableEntity properties are configured globally in ApplicationDbContext.OnModelCreating
        // No need to configure CreatedAt, UpdatedAt, CreatedBy, UpdatedBy, IsDeleted, DeletedAt, DeletedBy, RowVersion here

        // Configure Scopes as JSON column (EF Core 7+ feature)
        // Store as JSON for simplicity - no need for separate table
        var scopesNavigation = builder.Metadata.FindNavigation(nameof(ApiClient.Scopes));
        if (scopesNavigation != null)
        {
            scopesNavigation.SetField("_scopes");
        }

        // Store scopes as JSON in a single column using private backing field
        builder.Property<List<string>>("_scopes")
            .HasColumnName("Scopes")
            .IsRequired();

        // Configure JSON serialization
        builder.Property<List<string>>("_scopes")
            .HasConversion(
                scopes => System.Text.Json.JsonSerializer.Serialize(scopes, (System.Text.Json.JsonSerializerOptions?)null),
                json => System.Text.Json.JsonSerializer.Deserialize<List<string>>(json, (System.Text.Json.JsonSerializerOptions?)null) ?? new List<string>());
    }
}
