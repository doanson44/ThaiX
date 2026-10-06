namespace ThaiX.Domain.Common.Entities;

/// <summary>
/// Base class for entities that require audit trail.
/// Extends BaseEntity with CreatedBy, UpdatedBy, and soft delete support.
/// Audit fields are populated automatically by infrastructure interceptor.
/// </summary>
public abstract class BaseAuditableEntity : BaseEntity
{
    /// <summary>
    /// Who created this entity (UserId).
    /// Set automatically by infrastructure interceptor.
    /// </summary>
    public Guid? CreatedBy { get; protected set; }

    /// <summary>
    /// Who last modified this entity (UserId).
    /// Set automatically by infrastructure interceptor.
    /// </summary>
    public Guid? UpdatedBy { get; protected set; }

    /// <summary>
    /// Indicates whether this entity is soft-deleted.
    /// </summary>
    public bool IsDeleted { get; protected set; }

    /// <summary>
    /// When this entity was soft-deleted.
    /// </summary>
    public DateTime? DeletedAt { get; protected set; }

    /// <summary>
    /// Who soft-deleted this entity (UserId).
    /// Set automatically by infrastructure interceptor.
    /// </summary>
    public Guid? DeletedBy { get; protected set; }

    /// <summary>
    /// Marks this entity as deleted (soft delete).
    /// Call this from domain logic when business rules require deletion.
    /// Infrastructure will automatically set DeletedAt and DeletedBy.
    /// </summary>
    protected void Delete()
    {
        if (IsDeleted)
            return; // Already deleted

        IsDeleted = true;
        DeletedAt = DateTime.UtcNow;
        // DeletedBy set automatically by infrastructure interceptor
    }

    /// <summary>
    /// Restores a soft-deleted entity.
    /// Call this from domain logic when business rules allow restoration.
    /// </summary>
    public void Restore()
    {
        if (!IsDeleted)
            return; // Not deleted

        IsDeleted = false;
        DeletedAt = null;
        DeletedBy = null;
    }

    /// <summary>
    /// Sets audit fields.
    /// Called by infrastructure interceptor, not domain logic.
    /// </summary>
    internal void SetAuditFields(Guid? createdBy, Guid? updatedBy)
    {
        if (CreatedBy == null)
            CreatedBy = createdBy;

        UpdatedBy = updatedBy;
    }

    /// <summary>
    /// Sets soft delete audit fields.
    /// Called by infrastructure interceptor, not domain logic.
    /// </summary>
    internal void SetDeletedBy(Guid deletedBy)
    {
        DeletedBy = deletedBy;
    }
}
