using ABCD.Domain.Exceptions;

namespace ABCD.Domain;

public class Blog {
    public BlogId BlogId { get; }
    
    private string _name = string.Empty;
    public required string Name {
        get => _name;
        set {
            if (string.IsNullOrWhiteSpace(value))
                throw new InvalidArgumentException("Blog name cannot be null or empty.", nameof(value));

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
            throw new InvalidArgumentException("Blog domain cannot be null.", nameof(blogDomain));

        if (!_domains.Contains(blogDomain))
            _domains.Add(blogDomain);
    }

    public void RemoveDomain(BlogDomain blogDomain) {
        if (blogDomain == null)
            throw new InvalidArgumentException("Blog domain cannot be null.", nameof(blogDomain));

        var toRemove = _domains.FirstOrDefault(d => d.Equals(blogDomain));
        if (toRemove != null)
            _domains.Remove(toRemove);
    }

    public Blog(BlogId blogId) {
        BlogId = blogId ?? throw new InvalidArgumentException("BlogId cannot be null.", nameof(blogId));
    }
}
