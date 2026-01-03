namespace ABCD.Domain;
using ABCD.Domain.Exceptions;

public sealed class PathSegment : IEquatable<PathSegment>
{
    public string Value { get; }

    public PathSegment(string value)
    {
        if (!IsValid(value))
            throw new DomainValidationException($"Invalid PathSegment: '{value}'. It must be 3-50 chars, only alphanumeric or dash, cannot start or end with dash.", new ArgumentException("Invalid PathSegment.", nameof(value)));
        Value = value.ToLowerInvariant();
    }

    public static bool IsValid(string value)
    {
        if (string.IsNullOrWhiteSpace(value)) return false;
        if (value.Length < 3 || value.Length > 50) return false;
        if (value.StartsWith("-") || value.EndsWith("-")) return false;
        if (value.Contains("--")) return false;
        foreach (char c in value)
        {
            if (!(char.IsLetterOrDigit(c) || c == '-')) return false;
        }
        return true;
    }

    public override string ToString() => Value;

    public override bool Equals(object? obj) => Equals(obj as PathSegment);
    public bool Equals(PathSegment? other) => other is not null && Value.Equals(other.Value);
    public override int GetHashCode() => Value.GetHashCode();

    public static bool operator ==(PathSegment? left, PathSegment? right) => Equals(left, right);
    public static bool operator !=(PathSegment? left, PathSegment? right) => !Equals(left, right);
}
