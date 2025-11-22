using System.Text.RegularExpressions;

namespace ABCD.Domain {
    public class BlogDomain {
        public int BlogId { get; }
        public Domain Domain { get; set; }

        public BlogDomain(int blogId, Domain domain) {
            if (blogId <= 0)
                throw new ArgumentOutOfRangeException(nameof(blogId), "BlogId must be greater than 0.");

            if(string.IsNullOrWhiteSpace(domain?.Name))
                throw new ArgumentException("Domain cannot be null or whitespace.", nameof(domain));

            BlogId = blogId;
            Domain = domain;
        }
    }


    public class Domain {
        public string Name { get; }

        public Domain(string name) {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentNullException("Domain name cannot be null, empty or whitespace.", nameof(name));

            const string domainPattern = @"^(localhost|(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,})$";
            if (!Regex.IsMatch(name, domainPattern))
                throw new ArgumentException($"'{name}' is not a valid domain name.", nameof(name));

            Name = name.Trim().ToLowerInvariant();
        }

        public override string ToString() => Name;
        public override bool Equals(object? obj) => obj is Domain other && Name.Equals(other.Name, StringComparison.OrdinalIgnoreCase);
        public override int GetHashCode() => Name.GetHashCode();
    }
}
