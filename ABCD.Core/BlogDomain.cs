using System.Text.RegularExpressions;

namespace ABCD.Core {
    public class BlogDomain {
        public int BlogId { get; }
        public DomainName DomainName { get; set; }

        public BlogDomain(int blogId, DomainName domain) {
            if (blogId <= 0)
                throw new ArgumentOutOfRangeException(nameof(blogId), "BlogId must be greater than 0.");

            if(string.IsNullOrWhiteSpace(domain?.Value))
                throw new ArgumentException("Domain cannot be null or whitespace.", nameof(domain));

            BlogId = blogId;
            DomainName = domain;
        }

        private BlogDomain() { DomainName = new DomainName(""); }
    }


    public class DomainName {
        public string Value { get; }

        public DomainName(string name) {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Domain cannot be null or whitespace.", nameof(name));

            const string domainPattern = @"^(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,}$";
            if (!Regex.IsMatch(name, domainPattern))
                throw new ArgumentException($"'{name}' is not a valid domain name.", nameof(name));

            Value = name.Trim().ToLowerInvariant();
        }

        // EF Core requires a parameterless constructor for value objects
        private DomainName() { Value = string.Empty; }

        public override string ToString() => Value;

        // Value equality
        public override bool Equals(object? obj) => obj is DomainName other && Value == other.Value;
        public override int GetHashCode() => Value.GetHashCode();
    }

}
