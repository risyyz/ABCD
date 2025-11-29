namespace ABCD.Domain;

public enum FragmentType
{
    Text,
    Image,    
    Code,
    Html
}

public enum FragmentStatus
{
    Active,
    Inactive
}

/// <summary>
/// Rich domain model for post fragments with positioning and status behaviors
/// </summary>
public class Fragment 
{
    public int FragmentId { get; set; }
    public int PostId { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public FragmentType Type { get; set; }
    public int Position { get; set; }
    public FragmentStatus Status { get; set; } = FragmentStatus.Active;
    
    // Navigation property
    public Post? Post { get; set; }    
}
