namespace ABCD.Domain;
using ABCD.Domain.Exceptions;

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
    private Post? _parent;
    public Post? Parent
    {
        get => _parent;
        set
        {
            if (PostId != null)
            {
                var ancestor = value;
                while (ancestor != null)
                {
                    if (ancestor.PostId != null && ancestor.BlogId == BlogId && ancestor.PostId == PostId)
                        throw new ValidationException("A post cannot be its own ancestor.", new ArgumentException("Parent cannot create an ancestor cycle.", nameof(value)));
                    
                    ancestor = ancestor.Parent;
                }
            }
            _parent = value;
        }
    }

    public Post(BlogId blogId, string title)
    {
        Initialize(blogId, null, title, PostStatus.Draft, null);
    }

    public Post(BlogId blogId, PostId postId, string title, PostStatus status, DateTime? dateLastPublished = null)
    {
        if (postId == null)
            throw new ValidationException("PostId cannot be null.", new ArgumentNullException(nameof(postId)));
                
        Initialize(blogId, postId, title, status, dateLastPublished);
    }

    private void Initialize(BlogId blogId, PostId? postId, string title, PostStatus status, DateTime? dateLastPublished) {
        if (blogId == null)
            throw new ValidationException("BlogId cannot be null.", new ArgumentNullException(nameof(blogId)));
        
        if (string.IsNullOrWhiteSpace(title) || !ContainsWord(title))
            throw new ValidationException("Title must contain at least one word and cannot be null, empty, or whitespace.", new ArgumentException("Value must contain at least one word and cannot be null, empty, or whitespace.", nameof(title)));
        
        if (!Enum.IsDefined(typeof(PostStatus), status))
            throw new ValidationException("Status is required and must be a valid PostStatus.", new ArgumentException("Invalid PostStatus.", nameof(status)));

        if (status == PostStatus.Published && (dateLastPublished == null || dateLastPublished.Value == default(DateTime)))
            throw new ValidationException("DateLastPublished must be set when status is Published.", new ArgumentException("Value must be set when status is Published.", nameof(status)));

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

    public PublishEligibilityResult EligibleForPublishing() {
        var result = new PublishEligibilityResult();

        if (Status != PostStatus.Draft)
            result.AddReason("Post status must be Draft");

        if (PathSegment == null)
            result.AddReason("PathSegment must be set");

        // Gather all path segments from this post and its ancestors
        var pathSegments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        if (PathSegment != null)
            pathSegments.Add(PathSegment.Value);

        var ancestor = Parent;
        while (ancestor != null) {
            if (ancestor.Status != PostStatus.Published)
                result.AddReason("All ancestor posts must be Published");

            if (ancestor.PathSegment == null)
                result.AddReason("All ancestor posts must have a PathSegment");
            else if(!pathSegments.Add(ancestor.PathSegment.Value))
                result.AddReason($"Duplicate PathSegment value found in ancestor chain: '{ancestor.PathSegment.Value}'.");

            ancestor = ancestor.Parent;
        }
       
        return result;
    }

    public void Publish() {
        var eligibility = EligibleForPublishing();
        if (!eligibility.CanPublish) {
            throw new ValidationException($"Post cannot be published because it does not meet all publishing requirements.\n - {string.Join("\n - ", eligibility.Reasons)}");
        }
        Status = PostStatus.Published;
        DateLastPublished = DateTime.UtcNow;
    }

    public void UnPublish() {
        if (Status != PostStatus.Published) {
            throw new ValidationException("Post can only be unpublished if it is currently published.");
        }
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

public enum PostStatus {
    Draft,
    Published
}

public class PublishEligibilityResult {
    private readonly List<string> _reasons;
    public PublishEligibilityResult() {
        _reasons = new List<string>();
    }

    public bool CanPublish { get => _reasons.Count == 0; }
    public IReadOnlyList<string> Reasons { get => _reasons; }

    public void AddReason(string reason) {
        if (string.IsNullOrWhiteSpace(reason))
            return;

        if (!_reasons.Contains(reason, StringComparer.OrdinalIgnoreCase))
            _reasons.Add(reason);
    }
}