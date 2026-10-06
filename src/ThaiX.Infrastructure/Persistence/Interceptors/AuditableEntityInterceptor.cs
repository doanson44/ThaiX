using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ThaiX.Application.Common.Interfaces;
using ThaiX.Domain.Common.Entities;

namespace ThaiX.Infrastructure.Persistence.Interceptors;

/// <summary>
/// EF Core interceptor that automatically populates audit fields and manages timestamps.
/// - Sets CreatedAt/CreatedBy on Add
/// - Sets UpdatedAt/UpdatedBy on Update
/// - Sets DeletedAt/DeletedBy on soft delete
/// - Enforces UTC timestamps
/// </summary>
public sealed class AuditableEntityInterceptor : SaveChangesInterceptor
{
    private readonly ICurrentUserService _currentUserService;

    public AuditableEntityInterceptor(ICurrentUserService currentUserService)
    {
        _currentUserService = currentUserService;
    }

    public override InterceptionResult<int> SavingChanges(
        DbContextEventData eventData,
        InterceptionResult<int> result)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChanges(eventData, result);
    }

    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken cancellationToken = default)
    {
        UpdateEntities(eventData.Context);
        return base.SavingChangesAsync(eventData, result, cancellationToken);
    }

    private void UpdateEntities(DbContext? context)
    {
        if (context == null)
            return;

        var utcNow = DateTime.UtcNow;
        var currentUserId = _currentUserService.IsAuthenticated &&
                            _currentUserService.UserId != Guid.Empty
            ? _currentUserService.UserId
            : (Guid?)null;

        foreach (var entry in context.ChangeTracker.Entries<BaseEntity>())
        {
            if (entry.State == EntityState.Added)
            {
                // Set creation timestamp (all BaseEntity instances)
                entry.Property(nameof(BaseEntity.CreatedAt)).CurrentValue = utcNow;
            }

            if (entry.State == EntityState.Modified || entry.State == EntityState.Added)
            {
                // Set update timestamp (all BaseEntity instances)
                entry.Entity.SetUpdatedAt(utcNow);
                entry.Property(nameof(BaseEntity.RowVersion)).CurrentValue = Guid.NewGuid().ToByteArray();
            }

            // Handle BaseAuditableEntity-specific fields
            if (entry.Entity is BaseAuditableEntity auditableEntity)
            {
                if (entry.State == EntityState.Added)
                {
                    auditableEntity.SetAuditFields(currentUserId, null);
                }
                else if (entry.State == EntityState.Modified)
                {
                    auditableEntity.SetAuditFields(null, currentUserId);

                    // If IsDeleted changed to true, this is a soft delete
                    var isDeletedProperty = entry.Property(nameof(BaseAuditableEntity.IsDeleted));
                    if (isDeletedProperty.IsModified &&
                        (bool)isDeletedProperty.CurrentValue! == true &&
                        currentUserId.HasValue)
                    {
                        auditableEntity.SetDeletedBy(currentUserId.Value);
                        entry.Property(nameof(BaseAuditableEntity.DeletedAt)).CurrentValue = utcNow;
                    }
                }
            }
        }
    }
}
