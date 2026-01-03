using System;
using Xunit;
using ABCD.Domain;
using ABCD.Domain.Exceptions;

namespace ABCD.Domain.Tests
{
    public class PathSegmentTests
    {
        [Theory]
        [InlineData("abc")]
        [InlineData("abc-123")]
        [InlineData("a-b-c")]
        [InlineData("abc123def456")]
        [InlineData("abc-def-ghi")]
        public void Constructor_ValidValue_ShouldSetLowercaseValue(string input)
        {
            var segment = new PathSegment(input.ToUpper());
            Assert.Equal(input.ToLowerInvariant(), segment.Value);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("  ")]
        [InlineData("ab")]
        [InlineData("a")]
        [InlineData("-abc")]
        [InlineData("abc-")]
        [InlineData("abc--def")]
        [InlineData("abc def")]
        [InlineData("abc_def")]
        [InlineData("abc@def")]
        [InlineData("abc.def")]
        [InlineData("abc/def")]
        [InlineData("abc#def")]
        [InlineData("abc$def")]
        [InlineData("abc%def")]
        [InlineData("abc^def")]
        [InlineData("abc&def")]
        [InlineData("abc*def")]
        [InlineData("abc+def")]
        [InlineData("abc=def")]
        [InlineData("abc,def")]
        [InlineData("abc:def")]
        [InlineData("abc;def")]
        [InlineData("abc?def")]
        [InlineData("abc!def")]
        [InlineData("abc@def")]
        [InlineData("abc[def")]
        [InlineData("abc]def")]
        [InlineData("abc{def")]
        [InlineData("abc}def")]
        [InlineData("abc|def")]
        [InlineData("abc\\def")]
        [InlineData("abc~def")]
        [InlineData("abc`def")]
        public void Constructor_InvalidValue_ShouldThrow(string input)
        {
            var ex = Assert.Throws<DomainValidationException>(() => new PathSegment(input!));
            Assert.Contains("Invalid PathSegment", ex.Message);
        }

        [Theory]
        [InlineData("abc", "abc", true)]
        [InlineData("abc", "ABC", true)]
        [InlineData("abc-123", "abc-123", true)]
        [InlineData("abc-123", "ABC-123", true)]
        [InlineData("abc", "def", false)]
        [InlineData("abc-123", "abc-124", false)]
        public void Equals_ShouldBeCaseInsensitiveAndValueBased(string a, string b, bool expected)
        {
            var segA = new PathSegment(a);
            var segB = new PathSegment(b);
            Assert.Equal(expected, segA.Equals(segB));
            Assert.Equal(expected, segA == segB);
            Assert.Equal(!expected, segA != segB);
        }

        [Theory]
        [InlineData("abc-def")]
        [InlineData("ABC-DEF")]
        [InlineData("AbC-DeF")]
        [InlineData("aBc-dEf")]
        public void ToString_ReturnsValue_Lowercase(string input)
        {
            var seg = new PathSegment(input);
            Assert.Equal(input.ToLowerInvariant(), seg.ToString());
        }

        [Fact]
        public void GetHashCode_IsConsistentWithValue()
        {
            var seg1 = new PathSegment("abc-def");
            var seg2 = new PathSegment("ABC-DEF");
            Assert.Equal(seg1.GetHashCode(), seg2.GetHashCode());
        }

        [Theory]
        [InlineData("-abc")]
        [InlineData("abc-")]
        public void Constructor_ShouldThrow_WhenStartsOrEndsWithDash(string input)
        {
            var ex = Assert.Throws<DomainValidationException>(() => new PathSegment(input));
            Assert.Contains("Invalid PathSegment", ex.Message);
        }

        [Theory]
        [InlineData("")]
        [InlineData("a")]
        [InlineData("ab")]
        public void Constructor_ShouldThrow_WhenLessThanThreeChars(string input)
        {
            var ex = Assert.Throws<DomainValidationException>(() => new PathSegment(input));
            Assert.Contains("Invalid PathSegment", ex.Message);
        }

        [Theory]
        [InlineData("abc@def")]
        [InlineData("abc def")]
        [InlineData("abc_def")]
        [InlineData("abc.def")]
        [InlineData("abc/def")]
        [InlineData("abc#def")]
        [InlineData("abc$def")]
        [InlineData("abc%def")]
        [InlineData("abc^def")]
        [InlineData("abc&def")]
        [InlineData("abc*def")]
        [InlineData("abc+def")]
        [InlineData("abc=def")]
        [InlineData("abc,def")]
        [InlineData("abc:def")]
        [InlineData("abc;def")]
        [InlineData("abc?def")]
        [InlineData("abc!def")]
        [InlineData("abc[def")]
        [InlineData("abc]def")]
        [InlineData("abc{def")]
        [InlineData("abc}def")]
        [InlineData("abc|def")]
        [InlineData("abc\\def")]
        [InlineData("abc~def")]
        [InlineData("abc`def")]
        public void Constructor_ShouldThrow_WhenContainsNonAlphaNumericOrDash(string input)
        {
            var ex = Assert.Throws<DomainValidationException>(() => new PathSegment(input));
            Assert.Contains("Invalid PathSegment", ex.Message);
        }

        [Theory]
        [InlineData("abc--def")]
        [InlineData("a--b")]
        [InlineData("--abc")]
        [InlineData("abc--")]
        public void Constructor_ShouldThrow_WhenConsecutiveDashes(string input)
        {
            var ex = Assert.Throws<DomainValidationException>(() => new PathSegment(input));
            Assert.Contains("Invalid PathSegment", ex.Message);
        }
    }
}
