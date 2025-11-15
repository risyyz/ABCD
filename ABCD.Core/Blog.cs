namespace ABCD.Core;

/// <summary>
/// Rich domain model for blogs with post management and domain behaviors
/// </summary>
public class Blog {
    public required int BlogId { get; set; }
    private string _name = string.Empty;
    public required string Name {
        get => _name;
        set {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be null or empty.", nameof(Name));

            _name = value.Trim();
        }
    }

    private  string _description = string.Empty;
    public string Description {
        get => _description;
        set {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Description cannot be null or empty.", nameof(Description));

            _description = value.Trim();
        }
    }

    private readonly List<Post> _posts = new();
    public IReadOnlyCollection<Post> Posts => _posts;

    private readonly List<BlogDomain> _domains = new();
    public IReadOnlyCollection<BlogDomain> Domains => _domains;
    public void AddDomain(string name) {
        var domainName = new Domain(name);
        if (!_domains.Any(d => d.Domain.Equals(domainName)))
            _domains.Add(new BlogDomain(BlogId, domainName));
    }

    public void ClearDomains() {
        _domains.Clear();
    }
}
