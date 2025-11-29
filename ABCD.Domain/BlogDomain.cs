using System.Text.RegularExpressions;

namespace ABCD.Domain {
    public class BlogDomain {
        public string DomainName { get; }

        public BlogDomain(string domainName) {
            if (string.IsNullOrWhiteSpace(domainName))
                throw new ArgumentNullException("Domain name cannot be null, empty or whitespace.", nameof(domainName));

            const string domainPattern = @"^(localhost|(?:[a-zA-Z0-9](?:[a-zA-Z0-9-]{0,61}[a-zA-Z0-9])?\.)+[a-zA-Z]{2,})$";
            if (!Regex.IsMatch(domainName, domainPattern))
                throw new ArgumentException($"'{domainName}' is not a valid domain name.", nameof(domainName));

            DomainName = domainName.Trim().ToLowerInvariant();
        }

        public override string ToString() => DomainName;
        public override bool Equals(object? obj) => obj is BlogDomain other && DomainName.Equals(other.DomainName, StringComparison.OrdinalIgnoreCase);
        public override int GetHashCode() => DomainName.GetHashCode();
    }
}
