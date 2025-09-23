using ABCD.Core.Common;

namespace ABCD.Core;

public enum FragmentType
{
    Image,
    Paragraph,
    Code,
    Html,
    Text
}

public enum FragmentStatus
{
    Active,
    Inactive
}

/// <summary>
/// Rich domain model for post fragments with positioning and status behaviors
/// </summary>
public class Fragment : AuditableEntity
{
    public int Id { get; set; }
    public int PostId { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public FragmentType Type { get; set; }
    public int Position { get; set; }
    public FragmentStatus Status { get; set; } = FragmentStatus.Active;
    
    // Navigation property
    public Post? Post { get; set; }

    /// <summary>
    /// Activates the fragment, making it visible in published posts
    /// </summary>
    /// <param name="updatedBy">The user activating the fragment</param>
    public void Activate(string updatedBy)
    {
        Status = FragmentStatus.Active;
        UpdateAuditFields(updatedBy);
    }

    /// <summary>
    /// Deactivates the fragment, hiding it from published posts
    /// </summary>
    /// <param name="updatedBy">The user deactivating the fragment</param>
    public void Deactivate(string updatedBy)
    {
        Status = FragmentStatus.Inactive;
        UpdateAuditFields(updatedBy);
    }

    /// <summary>
    /// Determines if this fragment should be included in published posts
    /// </summary>
    public bool ShouldIncludeInPublishedPost()
    {
        return Status == FragmentStatus.Active;
    }

    /// <summary>
    /// Updates the fragment content and type
    /// </summary>
    /// <param name="content">New content</param>
    /// <param name="type">Fragment type</param>
    /// <param name="updatedBy">The user making the update</param>
    public void UpdateContent(string content, FragmentType type, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content cannot be empty", nameof(content));

        Content = content;
        Type = type;
        UpdateAuditFields(updatedBy);
    }

    /// <summary>
    /// Updates the fragment position
    /// </summary>
    /// <param name="newPosition">New position value</param>
    /// <param name="updatedBy">The user making the update</param>
    public void UpdatePosition(int newPosition, string updatedBy)
    {
        if (newPosition < 0)
            throw new ArgumentException("Position cannot be negative", nameof(newPosition));

        Position = newPosition;
        UpdateAuditFields(updatedBy);
    }
}
