namespace ABCD.Domain;

public enum FragmentType
{
    Code,
    Heading,
    Html,
    Image,  
    Text,
}

public class Fragment 
{
    private const int MinPosition = 1;

    public PostId PostId { get; }
    public FragmentType FragmentType { get; }
    public int Position { get; private set; }
    public string? Content { get; set; }
    public bool Active { get; private set; } = true;

    public Fragment(PostId postId, FragmentType type, int position)
    {
        PostId = postId ?? throw new ArgumentNullException(nameof(postId));
        if (position < MinPosition)
            throw new ArgumentOutOfRangeException(nameof(position), $"Position must be at least {MinPosition}.");

        FragmentType = type;        
        Position = position;
    }

    public void MoveUp()
    {
        if (Position == MinPosition)
            throw new InvalidOperationException($"Cannot move up. Position is already at minimum value {MinPosition}.");

        Position--;
    }

    public void MoveDown(int maxPosition)
    {
        if (Position >= maxPosition)
            throw new InvalidOperationException($"Cannot move down. Position is already at maximum value {maxPosition}.");
        
        Position++;
    }

    public void ToggleActive()
    {
        Active = !Active;
    }
}
