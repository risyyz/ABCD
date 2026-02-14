namespace ABCD.Domain;

public class VersionToken : IEquatable<VersionToken>
{
    public byte[] Value { get; }

    public VersionToken(byte[] value)
    {
        Value = value ?? Array.Empty<byte>();
    }

    public VersionToken(string base64)
    {
        Value = string.IsNullOrEmpty(base64) ? Array.Empty<byte>() : Convert.FromBase64String(base64);
    }

    public string AsBase64 => Convert.ToBase64String(Value);

    public bool Equals(VersionToken? other) =>
        other != null && Value.SequenceEqual(other.Value);

    public override bool Equals(object? obj) => obj is VersionToken other && Equals(other);

    public override int GetHashCode() =>
        Value != null && Value.Length >= 4 ? BitConverter.ToInt32(Value, 0) : 0;
}
