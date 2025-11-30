using System.Text.RegularExpressions;

namespace ABCD.Domain;

public class Blog {
    public BlogId BlogId { get; }
    
    private const int MinNameWordCount = 3;

    private string _name = string.Empty;
    public required string Name {
        get => _name;
        set {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Name cannot be null or empty.", nameof(Name));

            var trimmed = value.Trim();
            var wordCount = Regex.Matches(trimmed, "\\b\\w+\\b").Count;
            if (wordCount < MinNameWordCount)
                throw new ArgumentException($"Name must contain at least {MinNameWordCount} words.", nameof(Name));

            _name = trimmed;
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
            throw new ArgumentNullException(nameof(blogDomain));

        if (!_domains.Contains(blogDomain))
            _domains.Add(blogDomain);
    }

    public void RemoveDomain(BlogDomain blogDomain) {
        if (blogDomain == null)
            throw new ArgumentNullException(nameof(blogDomain));

        var toRemove = _domains.FirstOrDefault(d => d.Equals(blogDomain));
        if (toRemove != null)
            _domains.Remove(toRemove);
    }

    public Blog(BlogId blogId) {
        BlogId = blogId ?? throw new ArgumentNullException(nameof(blogId));
    }
}
