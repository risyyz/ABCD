using ABCD.Domain.Exceptions;

namespace ABCD.Domain;

public class Post {
    public VersionToken? Version { get; set; }
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

    //TODO: if post is published, should not allow to change path segment to avoid broken links
    //Do not allow changing path segment if child is published
    private PathSegment? _pathSegment;
    public PathSegment? PathSegment {
        get => _pathSegment;
        set => _pathSegment = value;
    }

    private readonly List<Fragment> _fragments = new();
    public IReadOnlyCollection<Fragment> Fragments => _fragments.AsReadOnly();
    public Fragment GetFragmentById(int fragmentId) =>    
        _fragments.FirstOrDefault(f => f.FragmentId!.Value == fragmentId)
               ?? throw new ArgumentException($"No fragment with id {fragmentId} exists in this post.", nameof(fragmentId));
    
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
                        throw new ArgumentException("A post cannot be its own ancestor.", nameof(value));
                    
                    ancestor = ancestor.Parent;
                }
            }
            _parent = value;
        } 
    }

    public Post(BlogId blogId, string title)
    {
        InitializeCoreAttributes(blogId, null, title, PostStatus.Draft, null);
    }

    public Post(BlogId blogId, PostId postId, string title, PostStatus status, DateTime? dateLastPublished = null, IEnumerable<Fragment>? fragments = null)
    {
        if (postId == null)
            throw new ArgumentNullException(nameof(postId), "PostId cannot be null.");

        InitializeFragments(fragments, postId);
        InitializeCoreAttributes(blogId, postId, title, status, dateLastPublished);        
    }

    private void InitializeCoreAttributes(BlogId blogId, PostId? postId, string title, PostStatus status, DateTime? dateLastPublished) {
        if (blogId == null)
            throw new ArgumentNullException(nameof(blogId), "BlogId cannot be null.");
        
        if (string.IsNullOrWhiteSpace(title) || !ContainsWord(title))
            throw new ArgumentException("Title must contain at least one word and cannot be null, empty, or whitespace.", nameof(title));

        if (!Enum.IsDefined(typeof(PostStatus), status))
            throw new ArgumentException("Status is required and must be a valid PostStatus.", nameof(status));

        if (status == PostStatus.Published && (dateLastPublished == null || dateLastPublished.Value == default(DateTime)))
            throw new ArgumentException("DateLastPublished must be set when status is Published.", nameof(dateLastPublished));

        BlogId = blogId;
        PostId = postId;
        Title = title;
        Status = status;
    }

    private void InitializeFragments(IEnumerable<Fragment>? fragments, PostId postId) {
        if(fragments?.Any() != true)
            return;

        if (fragments?.Any(f => f.FragmentId == null) == true)
            throw new InvalidFragmentException("All fragments must have an id.");

        if (fragments?.Any(f => postId != null && !postId.Equals(f.PostId)) == true)
            throw new InvalidFragmentException("Fragment's postid must match the post's id.");

        var sortedFragments = fragments!.OrderBy(f => f.Position).ToList();
        for(int i = 0; i < sortedFragments.Count; i++) {
            int expectedPosition = Fragment.MinPosition + i;
            if(sortedFragments[i].Position != expectedPosition) {
                throw new InvalidFragmentException($"Fragment positions must be consecutive starting from {Fragment.MinPosition}. Fragment '{sortedFragments[i].FragmentId}' has invalid position {sortedFragments[i].Position}.");
            }
            _fragments.Add(sortedFragments[i]);
        }
    }

    public void AddFragment(FragmentType fragmentType, int? position = null) {
        int fragmentCount = _fragments.Count;
        int insertPosition = position ?? (fragmentCount == 0 ? Fragment.MinPosition : fragmentCount + Fragment.MinPosition);
        if (insertPosition < Fragment.MinPosition || insertPosition > fragmentCount + Fragment.MinPosition)
            throw new IllegalOperationException($"Position must be between {Fragment.MinPosition} and {fragmentCount + Fragment.MinPosition}.");

        var fragment = new Fragment(this.PostId!, fragmentType, insertPosition);

        // Shift positions of fragments at or after the insert position
        foreach (var f in _fragments.Where(f => f.Position >= insertPosition))
        {
            f.MoveDown(fragmentCount + Fragment.MinPosition); // MoveDown will increment position
        }

        _fragments.Add(fragment);
        // Re-sort fragments by position
        _fragments.Sort((a, b) => a.Position.CompareTo(b.Position));
    }

    public void RemoveFragment(int fragmentId) {
        var fragment = _fragments.FirstOrDefault(f => f.FragmentId!.Value == fragmentId);
        if (fragment == null)
            throw new ArgumentException($"Post {PostId.Value} does not contain any fragment with id {fragmentId}.", nameof(fragmentId));

        int removedPosition = fragment.Position;
        _fragments.Remove(fragment);
        // Shift positions of fragments after the removed position
        foreach (var f in _fragments.Where(f => f.Position > removedPosition))
        {
            f.MoveUp(); // MoveUp will decrement position
        }
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
            throw new IllegalOperationException($"Post cannot be published because it does not meet all publishing requirements.\n - {string.Join("\n - ", eligibility.Reasons)}");
        }
        Status = PostStatus.Published;
        DateLastPublished = DateTime.UtcNow;
    }

    public void UnPublish() {
        if (Status != PostStatus.Published) {
            throw new IllegalOperationException("Post can only be unpublished if it is currently published.");
        }
        Status = PostStatus.Draft;
    }

    /// <summary>
    /// RETURN FragmentPositionChangeResult and hand it over to repositorylayer to update the database
    /// add version to both of post and fragment
    /// </summary>
    /// <param name="fragmentId"></param>
    /// <param name="newPosition"></param>
    public IEnumerable<Fragment> ChangeFragmentPosition(int fragmentId, int newPosition) {
        var fragment = _fragments.FirstOrDefault(f => f.FragmentId!.Value == fragmentId);
        if (fragment == null)
            throw new ArgumentException($"Post {PostId.Value} does not contain any fragment with id {fragmentId}.", nameof(fragmentId));

        ValidatePositionChange(fragment.Position, newPosition);
        Fragment impacted;
        if (newPosition < fragment.Position) 
            impacted = MoveFragmentUp(fragment!);
        else 
            impacted = MoveFragmentDown(fragment!);
        
        return [fragment, impacted];
    }

    private void ValidatePositionChange(int currentPosition, int newPosition) {
        if (currentPosition == newPosition)
            throw new IllegalOperationException("New position must be different from the current position.");

        if (_fragments.Count <= 1)
            throw new IllegalOperationException($"Cannot move fragment when {_fragments.Count} fragment exists.");

        if (Math.Abs(currentPosition - newPosition) != 1)
            throw new IllegalOperationException("Fragment can move by only one position at a time.");

        if (currentPosition < Fragment.MinPosition || currentPosition > _fragments.Count)
            throw new IllegalOperationException($"Current position must be between {Fragment.MinPosition} and {_fragments.Count}.");

        if (newPosition < Fragment.MinPosition || newPosition > _fragments.Count)
            throw new IllegalOperationException($"Invalid fragment position {newPosition}.");
    }

    private Fragment MoveFragmentUp(Fragment fragment) {
        var prevFragment = _fragments.First(f => f.Position == fragment.Position - 1);
        prevFragment.MoveDown(_fragments.Count);
        fragment.MoveUp();
        _fragments.Sort((a, b) => a.Position.CompareTo(b.Position));
        return prevFragment;
    }

    private Fragment MoveFragmentDown(Fragment fragment) {
        var nextFragment = _fragments.First(f => f.Position == fragment.Position + 1);
        nextFragment.MoveUp();
        fragment.MoveDown(_fragments.Count);
        _fragments.Sort((a, b) => a.Position.CompareTo(b.Position));        
        return nextFragment;
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