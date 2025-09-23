using ABCD.Core.Common;

namespace ABCD.Core;

/// <summary>
/// Rich domain model for blogs with post management and domain behaviors
/// </summary>
public class Blog : AuditableEntity
{
    private readonly List<BlogDomain> _domains = new();
    private readonly List<Post> _posts = new();

    public int BlogId { get; set; }
    public required string Title { get; set; }
    
    // Collections with rich behaviors
    public IReadOnlyList<BlogDomain> Domains => _domains.AsReadOnly();
    public IReadOnlyList<Post> Posts => _posts.AsReadOnly();

    /// <summary>
    /// Gets all published posts ordered by creation date (newest first)
    /// </summary>
    public IEnumerable<Post> PublishedPosts => 
        _posts.Where(p => p.IsPubliclyVisible())
               .OrderByDescending(p => p.CreatedDate);

    /// <summary>
    /// Gets all draft posts
    /// </summary>
    public IEnumerable<Post> DraftPosts => 
        _posts.Where(p => p.Status == PostStatus.Draft);

    /// <summary>
    /// Updates the blog title
    /// </summary>
    /// <param name="title">New title</param>
    /// <param name="updatedBy">The user making the update</param>
    public void UpdateTitle(string title, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty", nameof(title));

        Title = title;
        UpdateAuditFields(updatedBy);
    }

    /// <summary>
    /// Adds a new domain to the blog
    /// </summary>
    /// <param name="domain">Domain URL to add</param>
    /// <param name="updatedBy">The user adding the domain</param>
    public void AddDomain(string domain, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("Domain cannot be empty", nameof(domain));

        if (_domains.Any(d => d.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Domain '{domain}' already exists for this blog");

        var blogDomain = new BlogDomain
        {
            BlogId = BlogId,
            Domain = domain.ToLowerInvariant(),
            Blog = this
        };

        _domains.Add(blogDomain);
        UpdateAuditFields(updatedBy);
    }

    /// <summary>
    /// Removes a domain from the blog
    /// </summary>
    /// <param name="domain">Domain URL to remove</param>
    /// <param name="updatedBy">The user removing the domain</param>
    public void RemoveDomain(string domain, string updatedBy)
    {
        if (string.IsNullOrWhiteSpace(domain))
            throw new ArgumentException("Domain cannot be empty", nameof(domain));

        var existingDomain = _domains.FirstOrDefault(d => 
            d.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase));

        if (existingDomain == null)
            throw new ArgumentException($"Domain '{domain}' not found", nameof(domain));

        if (_domains.Count == 1)
            throw new InvalidOperationException("Cannot remove the last domain from a blog");

        _domains.Remove(existingDomain);
        UpdateAuditFields(updatedBy);
    }

    /// <summary>
    /// Adds a new post to the blog
    /// </summary>
    /// <param name="post">Post to add</param>
    /// <param name="updatedBy">The user adding the post</param>
    public void AddPost(Post post, string updatedBy)
    {
        if (post == null)
            throw new ArgumentNullException(nameof(post));

        // Check for SEO link uniqueness within the blog
        if (_posts.Any(p => p.SeoFriendlyLink.Equals(post.SeoFriendlyLink, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"A post with SEO link '{post.SeoFriendlyLink}' already exists in this blog");

        post.BlogId = BlogId;
        post.Blog = this;
        _posts.Add(post);
        UpdateAuditFields(updatedBy);
    }

    /// <summary>
    /// Removes a post from the blog
    /// </summary>
    /// <param name="postId">ID of post to remove</param>
    /// <param name="updatedBy">The user removing the post</param>
    public void RemovePost(int postId, string updatedBy)
    {
        var post = _posts.FirstOrDefault(p => p.PostId == postId);
        if (post == null)
            throw new ArgumentException("Post not found", nameof(postId));

        _posts.Remove(post);
        UpdateAuditFields(updatedBy);
    }

    /// <summary>
    /// Finds a post by its SEO-friendly link
    /// </summary>
    /// <param name="seoLink">SEO-friendly link</param>
    /// <returns>The post if found, null otherwise</returns>
    public Post? FindPostBySeoLink(string seoLink)
    {
        if (string.IsNullOrWhiteSpace(seoLink))
            return null;

        return _posts.FirstOrDefault(p => 
            p.SeoFriendlyLink.Equals(seoLink, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets posts by category
    /// </summary>
    /// <param name="category">Category name</param>
    /// <param name="publishedOnly">Whether to include only published posts</param>
    /// <returns>Posts in the specified category</returns>
    public IEnumerable<Post> GetPostsByCategory(string category, bool publishedOnly = true)
    {
        var posts = publishedOnly ? PublishedPosts : _posts;
        
        return posts.Where(p => 
            string.Equals(p.Category, category, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Checks if a domain belongs to this blog
    /// </summary>
    /// <param name="domain">Domain to check</param>
    /// <returns>True if the domain belongs to this blog</returns>
    public bool HasDomain(string domain)
    {
        return _domains.Any(d => 
            d.Domain.Equals(domain, StringComparison.OrdinalIgnoreCase));
    }
}

/// <summary>
/// Value object representing a blog domain
/// </summary>
public class BlogDomain
{
    public int BlogId { get; set; }
    public required string Domain { get; set; }
    public Blog? Blog { get; set; }
}
