namespace ABCD.Domain;

public class VersionToken : IEquatable<VersionToken>
{
    public byte[] Value { get; }

    public VersionToken(byte[] value)
    {
        Value = value ?? Array.Empty<byte>();
    }

    public VersionToken(string hexString)
    {
        Value = string.IsNullOrWhiteSpace(hexString) ? Array.Empty<byte>() : HexStringToBytes(hexString);
    }

    private byte[] HexStringToBytes(string hex)
    {
        if (hex.StartsWith("0x", StringComparison.OrdinalIgnoreCase))
            hex = hex.Substring(2);

        if (hex.Length % 2 != 0)
            throw new ArgumentException("Hex string must have an even length.");

        var bytes = new byte[hex.Length / 2];
        for (int i = 0; i < hex.Length; i += 2)
            bytes[i / 2] = Convert.ToByte(hex.Substring(i, 2), 16);

        return bytes;
    }

    public string HexString => "0x" + BitConverter.ToString(Value).Replace("-", "");

    public bool Equals(VersionToken? other) =>
        other != null && Value.SequenceEqual(other.Value);

    public override bool Equals(object? obj) => obj is VersionToken other && Equals(other);

    public override int GetHashCode() => Value != null && Value.Length >= 4 ? BitConverter.ToInt32(Value, 0) : 0;
    public override string ToString() => HexString;        
}
