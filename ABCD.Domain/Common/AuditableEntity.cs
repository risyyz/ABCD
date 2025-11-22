namespace ABCD.Domain.Common;

/// <summary>
/// Base class for auditable entities with created and updated tracking
/// </summary>
public abstract class AuditableEntity
{
    public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
    public DateTime LastUpdatedDate { get; set; } = DateTime.UtcNow;
    public required string CreatedBy { get; set; }
    public required string LastUpdatedBy { get; set; }

    /// <summary>
    /// Updates the audit fields for entity modification
    /// </summary>
    /// <param name="updatedBy">The user making the update</param>
    public virtual void UpdateAuditFields(string updatedBy)
    {
        LastUpdatedDate = DateTime.UtcNow;
        LastUpdatedBy = updatedBy;
    }
}