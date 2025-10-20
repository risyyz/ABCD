namespace ABCD.Core;

/// <summary>
/// Rich domain model for blogs with post management and domain behaviors
/// </summary>
public class Blog {
    public int BlogId { get; set; }
    public string Name { get; set; } = string.Empty;

    // Backing field for EF Core
    private readonly List<Post> _posts = new();
    public IReadOnlyCollection<Post> Posts => _posts;

    private readonly List<BlogDomain> _domains = new();
    public IReadOnlyCollection<BlogDomain> Domains => _domains;
}

public class BlogDomain {
    public int BlogId { get; }
    public string Domain { get; set; } = string.Empty;

    public BlogDomain(int blogId) {
        if (blogId <= 0)
            throw new ArgumentOutOfRangeException(nameof(blogId), "BlogId must be greater than 0.");

        BlogId = blogId;
    }

    private BlogDomain() { }
}
