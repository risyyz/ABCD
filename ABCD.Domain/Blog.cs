using ABCD.Domain.Exceptions;

namespace ABCD.Domain;

public class Blog {
    public BlogId BlogId { get; }
    
    private string _name = string.Empty;
    public required string Name {
        get => _name;
        set {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Blog name cannot be null or empty.", nameof(value));

            _name = value.Trim();
        }
    }

    private string? _description;
    public string? Description {
        get => _description;
        set => _description = value?.Trim();
    }

    private readonly List<BlogDomain> _domains = new();
    public IReadOnlyCollection<BlogDomain> Domains => _domains;
    public void AddDomain(BlogDomain blogDomain) {
        if(blogDomain == null)
            throw new ArgumentNullException(nameof(blogDomain), "Blog domain cannot be null.");

        if (!_domains.Contains(blogDomain))
            _domains.Add(blogDomain);
    }

    public void RemoveDomain(BlogDomain blogDomain) {
        if (blogDomain == null)
            throw new ArgumentNullException(nameof(blogDomain), "Blog domain cannot be null.");

        var toRemove = _domains.FirstOrDefault(d => d.Equals(blogDomain));
        if (toRemove != null)
            _domains.Remove(toRemove);
    }

    public Blog(BlogId blogId) {
        BlogId = blogId ?? throw new ArgumentNullException(nameof(blogId), "BlogId cannot be null.");
    }
}
