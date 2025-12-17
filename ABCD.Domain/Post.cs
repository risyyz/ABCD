namespace ABCD.Domain;
using ABCD.Domain.Exceptions;

public enum PostStatus
{
    Draft,
    Published
}

public class Post {
    public BlogId BlogId { get; private set; }
    public PostId? PostId { get; private set; }
    public PostStatus Status { get; private set; }
    public DateTime? DateLastPublished { get; private set; }

    private string _title;
    public string Title {
        get => _title;
        set {
            if (string.IsNullOrWhiteSpace(value) || !ContainsWord(value))
                throw new ValidationException("Title must contain at least one word and cannot be null, empty, or whitespace.", new ArgumentException("Value must contain at least one word and cannot be null, empty, or whitespace.", nameof(value)));
            _title = value;
        }
    }

    private PathSegment? _pathSegment;
    public PathSegment? PathSegment {
        get => _pathSegment;
        set => _pathSegment = value;
    }

    private readonly List<Fragment> _fragments = new();
    public IReadOnlyCollection<Fragment> Fragments => _fragments.AsReadOnly();

    public Post(BlogId blogId, string title)
    {
        Initialize(blogId, null, title, PostStatus.Draft);
    }

    public Post(BlogId blogId, PostId postId, string title, PostStatus status)
    {
        if (postId == null)
            throw new ValidationException("PostId cannot be null.", new ArgumentNullException(nameof(postId)));
        if (status == PostStatus.Published)
            throw new ValidationException("DateLastPublished must be set when status is Published.", new ArgumentException("Value must be set when status is Published.", nameof(status)));
        Initialize(blogId, postId, title, status);
    }

    private void Initialize(BlogId blogId, PostId? postId, string title, PostStatus status) {
        if (blogId == null)
            throw new ValidationException("BlogId cannot be null.", new ArgumentNullException(nameof(blogId)));
        if (string.IsNullOrWhiteSpace(title) || !ContainsWord(title))
            throw new ValidationException("Title must contain at least one word and cannot be null, empty, or whitespace.", new ArgumentException("Value must contain at least one word and cannot be null, empty, or whitespace.", nameof(title)));
        if (!Enum.IsDefined(typeof(PostStatus), status))
            throw new ValidationException("Status is required and must be a valid PostStatus.", new ArgumentException("Invalid PostStatus.", nameof(status)));
        BlogId = blogId;
        PostId = postId;
        Title = title;
        Status = status;
    }

    public void AddFragment(FragmentType fragmentType, string? content, int? position = null)
    {
        int fragmentCount = _fragments.Count;
        int insertPosition = position ?? (fragmentCount == 0 ? Fragment.MinPosition : fragmentCount + Fragment.MinPosition);
        if (insertPosition < Fragment.MinPosition || insertPosition > fragmentCount + Fragment.MinPosition)
            throw new FragmentPositionException($"Position must be between {Fragment.MinPosition} and {fragmentCount + Fragment.MinPosition}.");

        var fragment = new Fragment(this.PostId!, fragmentType, insertPosition) { Content = content };

        // Shift positions of fragments at or after the insert position
        foreach (var f in _fragments.Where(f => f.Position >= insertPosition))
        {
            f.MoveDown(fragmentCount + Fragment.MinPosition); // MoveDown will increment position
        }

        _fragments.Add(fragment);
        // Re-sort fragments by position
        _fragments.Sort((a, b) => a.Position.CompareTo(b.Position));
    }

    public void Publish() {
        if (Status == PostStatus.Draft) {
            Status = PostStatus.Published;
            DateLastPublished = DateTime.UtcNow;
        }
    }

    public void UnPublish() {
        Status = PostStatus.Draft;
    }

    public void MoveFragmentUp(int currentFragmentPosition)
    {
        ValidateFragmentMovement(currentFragmentPosition);
        if (currentFragmentPosition == Fragment.MinPosition)
            throw new FragmentPositionException($"Fragment at position {currentFragmentPosition} is already at the top.");

        var fragment = _fragments.FirstOrDefault(f => f.Position == currentFragmentPosition);
        if (fragment == null)
            throw new FragmentPositionException($"No fragment found at position {currentFragmentPosition}.");

        var prevFragment = _fragments.First(f => f.Position == currentFragmentPosition - 1);
        prevFragment.MoveDown(_fragments.Count);
        fragment.MoveUp();
        _fragments.Sort((a, b) => a.Position.CompareTo(b.Position));
    }

    public void MoveFragmentDown(int currentFragmentPosition)
    {
        ValidateFragmentMovement(currentFragmentPosition);
        if (currentFragmentPosition == _fragments.Count)
            throw new FragmentPositionException($"Fragment at position {currentFragmentPosition} is already at the bottom.");

        var fragment = _fragments.FirstOrDefault(f => f.Position == currentFragmentPosition);
        if (fragment == null)
            throw new FragmentPositionException($"No fragment found at position {currentFragmentPosition}.");

        var nextFragment = _fragments.First(f => f.Position == currentFragmentPosition + 1);
        nextFragment.MoveUp();
        fragment.MoveDown(_fragments.Count);
        _fragments.Sort((a, b) => a.Position.CompareTo(b.Position));
    }

    private void ValidateFragmentMovement(int currentFragmentPosition) {
        if (_fragments.Count <= 1)
            throw new FragmentPositionException($"Cannot move fragment when {_fragments.Count} fragment exists.");

        if (currentFragmentPosition < Fragment.MinPosition || currentFragmentPosition > _fragments.Count)
            throw new FragmentPositionException($"Position must be between {Fragment.MinPosition} and {_fragments.Count}.");
    }

    private bool ContainsWord(string input) {
        return input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Any(w => w.Length > 0);
    }
}