namespace ABCD.Domain.Tests
{
    public class VersionTokenTests
    {
        [Fact]
        public void Constructor_WithByteArray_SetsValue()
        {
            var bytes = new byte[] { 1, 2, 3, 4 };
            var token = new VersionToken(bytes);
            Assert.Equal(bytes, token.Value);
        }

        [Fact]
        public void Constructor_WithNullByteArray_SetsEmptyArray()
        {
            var token = new VersionToken((byte[])null!);
            Assert.NotNull(token.Value);
            Assert.Empty(token.Value);
        }

        [Fact]
        public void Constructor_WithHexString_SetsValue()
        {
            var bytes = new byte[] { 10, 20, 30, 40 };
            var hex = "0x0A141E28";
            var token = new VersionToken(hex);
            Assert.Equal(bytes, token.Value);
        }

        [Fact]
        public void Constructor_WithNullOrEmptyOrInvalidHexString_ThrowsOrSetsEmptyArray()
        {
            // Null, empty, or whitespace should result in empty array
            var token1 = new VersionToken((string)null!);
            var token2 = new VersionToken("");
            var token3 = new VersionToken("   ");
            Assert.Empty(token1.Value);
            Assert.Empty(token2.Value);
            Assert.Empty(token3.Value);

            // Odd-length hex string should throw
            Assert.Throws<ArgumentException>(() => new VersionToken("0x123"));
            Assert.Throws<ArgumentException>(() => new VersionToken("123"));
        }

        [Fact]
        public void HexString_ReturnsCorrectHexString()
        {
            var bytes = new byte[] { 5, 6, 7, 8 };
            var token = new VersionToken(bytes);
            var expected = "0x05060708";
            Assert.Equal(expected, token.HexString);
        }

        [Fact]
        public void Equals_ReturnsTrueForSameValue()
        {
            var bytes = new byte[] { 1, 2, 3, 4 };
            var token1 = new VersionToken(bytes);
            var token2 = new VersionToken(bytes);
            Assert.True(token1.Equals(token2));
            Assert.True(token1.Equals((object)token2));
        }

        [Fact]
        public void Equals_ReturnsFalseForDifferentValue()
        {
            var token1 = new VersionToken(new byte[] { 1, 2, 3 });
            var token2 = new VersionToken(new byte[] { 4, 5, 6 });
            Assert.False(token1.Equals(token2));
        }

        [Fact]
        public void GetHashCode_IsConsistentForSameValue()
        {
            var bytes = new byte[] { 1, 2, 3, 4 };
            var token1 = new VersionToken(bytes);
            var token2 = new VersionToken(bytes);
            Assert.Equal(token1.GetHashCode(), token2.GetHashCode());
        }
    }
}
