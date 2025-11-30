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

    public Post(BlogId blogId, string title) {
        Initialize(blogId, null, title, PostStatus.Draft);
    }

    public Post(BlogId blogId, PostId postId, string title, PostStatus status) {
        if (postId == null)
            throw new ArgumentNullException(nameof(postId));

        Initialize(blogId, postId, title, status);
    }

    private void Initialize(BlogId blogId, PostId? postId, string title, PostStatus status) {
        BlogId = blogId ?? throw new ArgumentNullException(nameof(blogId));
        PostId = postId;
        Title = title;
        Status = status;
        DateLastPublished = null;
    }

    public void Publish() {
        if (Status == PostStatus.Draft) {
            Status = PostStatus.Published;
            DateLastPublished = DateTime.UtcNow;
        }
    }

    public void SetAsDraft() {
        Status = PostStatus.Draft;
    }

    private bool ContainsWord(string input) {
        return input.Split(' ', StringSplitOptions.RemoveEmptyEntries).Any(w => w.Length > 0);
    }
}