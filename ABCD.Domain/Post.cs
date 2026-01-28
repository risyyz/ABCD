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
                throw new DomainValidationException("Title must contain at least one word and cannot be null, empty, or whitespace.", new ArgumentException("Value must contain at least one word and cannot be null, empty, or whitespace.", nameof(value)));
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
                        throw new DomainValidationException("A post cannot be its own ancestor.", new ArgumentException("Parent cannot create an ancestor cycle.", nameof(value)));
                    
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

    public Post(BlogId blogId, PostId postId, string title, PostStatus status, DateTime? dateLastPublished = null, IEnumerable<Fragment>? fragments = null)
    {
        if (postId == null)
            throw new DomainValidationException("PostId cannot be null.", new ArgumentNullException(nameof(postId)));
                
        Initialize(blogId, postId, title, status, dateLastPublished, fragments);        
    }

    private void Initialize(BlogId blogId, PostId? postId, string title, PostStatus status, DateTime? dateLastPublished, IEnumerable<Fragment>? fragments = null) {
        if (blogId == null)
            throw new DomainValidationException("BlogId cannot be null.", new ArgumentNullException(nameof(blogId)));
        
        if (string.IsNullOrWhiteSpace(title) || !ContainsWord(title))
            throw new DomainValidationException("Title must contain at least one word and cannot be null, empty, or whitespace.", new ArgumentException("Value must contain at least one word and cannot be null, empty, or whitespace.", nameof(title)));
        
        if (!Enum.IsDefined(typeof(PostStatus), status))
            throw new DomainValidationException("Status is required and must be a valid PostStatus.", new ArgumentException("Invalid PostStatus.", nameof(status)));

        if (status == PostStatus.Published && (dateLastPublished == null || dateLastPublished.Value == default(DateTime)))
            throw new DomainValidationException("DateLastPublished must be set when status is Published.", new ArgumentException("Value must be set when status is Published.", nameof(status)));

        if(fragments?.Any(f => f.FragmentId == null) == true)
            throw new DomainValidationException("All fragments must have an id.");

        if(fragments?.Any(f => postId != null && f.PostId != postId) == true)
            throw new DomainValidationException("Fragments within a post cannot have different ids.");
        
        EnsureConsecutiveFragmentPositions(fragments);        

        BlogId = blogId;
        PostId = postId;
        Title = title;
        Status = status;
    }

    private void EnsureConsecutiveFragmentPositions(IEnumerable<Fragment>? fragments) {
        if(fragments?.Any() != true)
            return;

        var sortedFragments = fragments.OrderBy(f => f.Position).ToList();
        for(int i = 0; i < sortedFragments.Count; i++) {
            int expectedPosition = Fragment.MinPosition + i;
            if(sortedFragments[i].Position != expectedPosition) {
                throw new DomainValidationException($"Fragment positions must be consecutive starting from {Fragment.MinPosition}. Fragment '{sortedFragments[i].FragmentId}' has invalid position {sortedFragments[i].Position}.");
            }
            _fragments.Add(sortedFragments[i]);
        }
    }

    public void AddFragment(FragmentType fragmentType, string? content, int? position = null) {
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

        if (PostId == null)
            result.AddReason("Post does not have a valid id");

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
            throw new DomainValidationException($"Post cannot be published because it does not meet all publishing requirements.\n - {string.Join("\n - ", eligibility.Reasons)}");
        }
        Status = PostStatus.Published;
        DateLastPublished = DateTime.UtcNow;
    }

    public void UnPublish() {
        if (Status != PostStatus.Published) {
            throw new DomainValidationException("Post can only be unpublished if it is currently published.");
        }
        Status = PostStatus.Draft;
    }

    public void ChangeFragmentPosition(int currentPosition, int newPosition) {
        ValidatePositionChange(currentPosition, newPosition);
        var fragment = _fragments.FirstOrDefault(f => f.Position == currentPosition);
        if (newPosition < currentPosition) {
            MoveFragmentUp(fragment!);
        } else {
            MoveFragmentDown(fragment!);
        }
    }

    private void ValidatePositionChange(int currentPosition, int newPosition) {
        if (currentPosition == newPosition)
            throw new FragmentPositionException("New position must be different from the current position.");

        if (_fragments.Count <= 1)
            throw new FragmentPositionException($"Cannot move fragment when {_fragments.Count} fragment exists.");

        if (Math.Abs(currentPosition - newPosition) != 1)
            throw new FragmentPositionException("Fragment can move by only one position at a time.");

        if (currentPosition < Fragment.MinPosition || currentPosition > _fragments.Count)
            throw new FragmentPositionException($"Current position must be between {Fragment.MinPosition} and {_fragments.Count}.");

        if (newPosition < Fragment.MinPosition || newPosition > _fragments.Count)
            throw new FragmentPositionException($"New position must be between {Fragment.MinPosition} and {_fragments.Count}.");
    }

    private void MoveFragmentUp(Fragment fragment) {
        
        var prevFragment = _fragments.First(f => f.Position == fragment.Position - 1);
        prevFragment.MoveDown(_fragments.Count);
        fragment.MoveUp();
        _fragments.Sort((a, b) => a.Position.CompareTo(b.Position));
    }

    private void MoveFragmentDown(Fragment fragment) {
        var nextFragment = _fragments.First(f => f.Position == fragment.Position + 1);
        nextFragment.MoveUp();
        fragment.MoveDown(_fragments.Count);
        _fragments.Sort((a, b) => a.Position.CompareTo(b.Position));
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