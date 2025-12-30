using System.Text.RegularExpressions;
using ABCD.Domain.Exceptions;

namespace ABCD.Domain {
    public class BlogDomain {
        public string DomainName { get; }

        public BlogDomain(string domainName) {
            if (string.IsNullOrWhiteSpace(domainName))
                throw new DomainValidationException("Domain name cannot be null, empty or whitespace.", new ArgumentNullException(nameof(domainName)));

            const string domainPattern = @"^(localhost|[a-zA-Z0-9-]+(\.[a-zA-Z0-9-]+)*\.[a-zA-Z]{2,})$";
            if (!Regex.IsMatch(domainName.Trim(), domainPattern, RegexOptions.IgnoreCase))
                throw new DomainValidationException($"'{domainName}' is not a valid domain name.", new ArgumentException($"'{domainName}' is not a valid domain name.", nameof(domainName)));

            DomainName = domainName.Trim().ToLowerInvariant();
        }

        public override string ToString() => DomainName;
        public override bool Equals(object? obj) => obj is BlogDomain other && DomainName.Equals(other.DomainName, StringComparison.OrdinalIgnoreCase);
        public override int GetHashCode() => DomainName.GetHashCode();
    }
}
