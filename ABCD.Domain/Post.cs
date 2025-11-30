namespace ABCD.Domain;

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
                throw new ArgumentException("Title must contain at least one word and cannot be null, empty, or whitespace.", nameof(value));
            _title = value;
        }
    }

    private readonly List<Fragment> _fragments = new();
    public IReadOnlyCollection<Fragment> Fragments => _fragments.AsReadOnly();

    public Post(BlogId blogId, string title) {
        Initialize(blogId, null, title, PostStatus.Draft, null);
    }

    public Post(BlogId blogId, PostId postId, string title, PostStatus status, DateTime? dateLastPublished) {
        if (postId == null)
            throw new ArgumentNullException(nameof(postId));

        if (status == PostStatus.Published && (!dateLastPublished.HasValue || dateLastPublished.Value == default))
            throw new ArgumentException("DateLastPublished must be set when status is Published.", nameof(dateLastPublished));
        
        Initialize(blogId, postId, title, status, dateLastPublished);
    }

    private void Initialize(BlogId blogId, PostId? postId, string title, PostStatus status, DateTime? dateLastPublished) {
        BlogId = blogId ?? throw new ArgumentNullException(nameof(blogId));
        PostId = postId;
        Title = title;
        Status = status;
        DateLastPublished = dateLastPublished;
    }

    public void AddFragment(FragmentType fragmentType, string? content, int? position = null)
    {
        int fragmentCount = _fragments.Count;
        int insertPosition = position ?? (fragmentCount == 0 ? 1 : fragmentCount + 1);
        if (insertPosition < 1 || insertPosition > fragmentCount + 1)
            throw new ArgumentOutOfRangeException(nameof(position), $"Position must be between 1 and {fragmentCount + 1}.");

        var fragment = new Fragment(this.PostId!, fragmentType, insertPosition) { Content = content };

        // Shift positions of fragments at or after the insert position
        foreach (var f in _fragments.Where(f => f.Position >= insertPosition))
        {
            f.MoveDown(fragmentCount + 1); // MoveDown will increment position
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

    private bool ContainsWord(string input) {
        return input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Any(w => w.Length > 0);
    }
}