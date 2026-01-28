namespace ABCD.Domain;

using ABCD.Domain.Exceptions;

public enum FragmentType
{
    Code,
    Heading,
    Table,
    Image,  
    RichText,
}

public class Fragment 
{
    public static readonly int MinPosition = 1;
    public FragmentId? FragmentId { get; }
    public PostId PostId { get; }
    public FragmentType FragmentType { get; }
    public int Position { get; private set; }
    public string? Content { get; set; }
    public bool Active { get; private set; } = true;

    public Fragment(PostId postId, FragmentType type, int position)
    {
        PostId = postId ?? throw new DomainValidationException("PostId cannot be null.", new ArgumentNullException(nameof(postId)));
        if (position < MinPosition)
            throw new FragmentPositionException($"Position must be at least {MinPosition}.");

        FragmentType = type;        
        Position = position;
    }

    public Fragment(FragmentId fragmentId, PostId postId, FragmentType type, int position) : this(postId, type, position) {
        FragmentId = fragmentId ?? throw new DomainValidationException("FragmentId cannot be null.", new ArgumentNullException(nameof(fragmentId)));        
    }

    public void MoveUp()
    {
        if (Position == MinPosition)
            throw new FragmentPositionException($"Cannot move up. Position is already at minimum value {MinPosition}.");

        Position--;
    }

    public void MoveDown(int maxPosition)
    {
        if (Position >= maxPosition)
            throw new FragmentPositionException($"Cannot move down. Position is already at maximum value {maxPosition}.");
        
        Position++;
    }

    public void ToggleActive()
    {
        Active = !Active;
    }
}
