using ABCD.Domain.Exceptions;

namespace ABCD.Domain;

public class Series {
    public SeriesId? SeriesId { get; private set; }
    public BlogId BlogId { get; private set; }
    public VersionToken? Version { get; set; }
    public SeriesStatus Status { get; private set; }
    public DateTime? DateLastPublished { get; private set; }

    private string _title;
    public string Title {
        get => _title;
        set {
            if (string.IsNullOrWhiteSpace(value) || !value.Split(' ', StringSplitOptions.RemoveEmptyEntries).Any(w => w.Length > 0))
                throw new ArgumentException("Title must contain at least one word and cannot be null, empty, or whitespace.", nameof(value));
            _title = value;
        }
    }

    private string? _description;
    public string? Description {
        get => _description;
        set => _description = value?.Trim();
    }

    private PathSegment? _pathSegment;
    public PathSegment? PathSegment {
        get => _pathSegment;
        set => _pathSegment = value;
    }

    private readonly List<Post> _posts = new();
    public IReadOnlyCollection<Post> Posts => _posts.AsReadOnly();

    public Series(BlogId blogId, string title) {
        BlogId = blogId ?? throw new ArgumentNullException(nameof(blogId), "BlogId cannot be null.");
        Title = title;
        Status = SeriesStatus.Draft;
    }

    public Series(BlogId blogId, SeriesId seriesId, string title, SeriesStatus status, DateTime? dateLastPublished = null, IEnumerable<Post>? posts = null) {
        BlogId = blogId ?? throw new ArgumentNullException(nameof(blogId), "BlogId cannot be null.");
        SeriesId = seriesId ?? throw new ArgumentNullException(nameof(seriesId), "SeriesId cannot be null.");

        if (!Enum.IsDefined(typeof(SeriesStatus), status))
            throw new ArgumentException("Status must be a valid SeriesStatus.", nameof(status));

        if (status == SeriesStatus.Published && (dateLastPublished == null || dateLastPublished.Value == default(DateTime)))
            throw new ArgumentException("DateLastPublished must be set when status is Published.", nameof(dateLastPublished));

        Title = title;
        Status = status;
        DateLastPublished = dateLastPublished;

        if (posts != null) {
            _posts.AddRange(posts);
            _posts.Sort((a, b) => (a.SeriesPosition ?? 0).CompareTo(b.SeriesPosition ?? 0));
        }
    }

    public void AddPost(Post post, int position) {
        if (post == null)
            throw new ArgumentNullException(nameof(post), "Post cannot be null.");

        if (post.BlogId != BlogId)
            throw new ArgumentException("Post must belong to the same blog as the series.", nameof(post));

        if (post.SeriesId != null && post.SeriesId != SeriesId)
            throw new IllegalOperationException("Post already belongs to another series.");

        if (_posts.Any(p => p.PostId != null && post.PostId != null && p.PostId == post.PostId))
            throw new IllegalOperationException("Post is already in this series.");

        if (position < 1 || position > _posts.Count + 1)
            throw new ArgumentOutOfRangeException(nameof(position), $"Position must be between 1 and {_posts.Count + 1}.");

        // Shift positions of posts at or after the insert position
        foreach (var p in _posts.Where(p => p.SeriesPosition >= position))
            p.SetSeriesPosition(p.SeriesPosition!.Value + 1);

        post.AssignToSeries(SeriesId!, position);
        _posts.Add(post);
        _posts.Sort((a, b) => (a.SeriesPosition ?? 0).CompareTo(b.SeriesPosition ?? 0));
    }

    public void RemovePost(Post post) {
        if (post == null)
            throw new ArgumentNullException(nameof(post), "Post cannot be null.");

        var existing = _posts.FirstOrDefault(p => p.PostId != null && post.PostId != null && p.PostId == post.PostId);
        if (existing == null)
            throw new IllegalOperationException("Post is not in this series.");

        int removedPosition = existing.SeriesPosition!.Value;
        _posts.Remove(existing);
        existing.RemoveFromSeries();

        // Shift positions of posts after the removed position
        foreach (var p in _posts.Where(p => p.SeriesPosition > removedPosition))
            p.SetSeriesPosition(p.SeriesPosition!.Value - 1);
    }

    public void Publish() {
        if (Status != SeriesStatus.Draft)
            throw new IllegalOperationException("Series can only be published from Draft status.");

        if (PathSegment == null)
            throw new IllegalOperationException("PathSegment must be set before publishing.");

        Status = SeriesStatus.Published;
        DateLastPublished = DateTime.UtcNow;
    }

    public void UnPublish() {
        if (Status != SeriesStatus.Published)
            throw new IllegalOperationException("Series can only be unpublished if it is currently published.");

        Status = SeriesStatus.Draft;
    }
}

public enum SeriesStatus {
    Draft,
    Published
}
